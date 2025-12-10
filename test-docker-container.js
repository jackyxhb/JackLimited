#!/usr/bin/env node

/**
 * Docker Container Test Runner
 * TDD/BDD Test Suite for JackLimited Docker Images
 *
 * This script provides programmatic testing of Docker containers
 * following Test-Driven Development and Behavior-Driven Development principles.
 */

const { execSync, spawn } = require('child_process');
const fs = require('fs');
const path = require('path');

// Configuration
const CONFIG = {
    imageName: 'jacklimited:test',
    containerName: `jacklimited-test-${Date.now()}`,
    dbContainerName: null,
    networkName: null,
    apiBaseUrl: 'http://localhost:8081',
    testTimeout: 30000,
    healthCheckInterval: 2000,
    maxRetries: 30
};

// Test results tracking
const testResults = {
    total: 0,
    passed: 0,
    failed: 0,
    skipped: 0,
    errors: []
};

// Logging utilities
const colors = {
    reset: '\x1b[0m',
    red: '\x1b[31m',
    green: '\x1b[32m',
    yellow: '\x1b[33m',
    blue: '\x1b[34m',
    magenta: '\x1b[35m',
    cyan: '\x1b[36m'
};

function log(level, message) {
    const timestamp = new Date().toISOString();
    const color = colors[level] || colors.reset;
    console.log(`${color}[${timestamp}] ${level.toUpperCase()}: ${message}${colors.reset}`);
}

function logTest(testName, status, details = '') {
    testResults.total++;
    const icon = status === 'PASS' ? '✅' : status === 'FAIL' ? '❌' : '⏭️';
    log(status === 'PASS' ? 'green' : status === 'FAIL' ? 'red' : 'yellow',
        `${icon} ${testName}: ${details}`);

    if (status === 'PASS') testResults.passed++;
    else if (status === 'FAIL') testResults.failed++;
    else testResults.skipped++;
}

// Docker utilities
class DockerManager {
    static execute(command, options = {}) {
        try {
            const result = execSync(command, {
                encoding: 'utf8',
                stdio: options.silent ? 'pipe' : 'inherit',
                ...options
            });
            return { success: true, output: result };
        } catch (error) {
            return { success: false, error: error.message, code: error.status };
        }
    }

    static async waitForHealth(endpoint, timeout = CONFIG.testTimeout) {
        const startTime = Date.now();

        while (Date.now() - startTime < timeout) {
            try {
                const response = await fetch(endpoint);
                if (response.ok) {
                    return true;
                }
            } catch (error) {
                // Continue waiting
            }
            await new Promise(resolve => setTimeout(resolve, CONFIG.healthCheckInterval));
        }

        return false;
    }
}

// Test suite
class DockerTestSuite {
    constructor() {
        this.dbContainerName = `${CONFIG.containerName}-db`;
        this.networkName = `${CONFIG.containerName}-network`;
    }

    async setup() {
        log('cyan', 'Setting up test environment...');

        // Create network
        DockerManager.execute(`docker network create ${this.networkName}`);

        // Start PostgreSQL
        log('blue', 'Starting PostgreSQL database...');
        const dbResult = DockerManager.execute(`
            docker run -d
            --name ${this.dbContainerName}
            --network ${this.networkName}
            --network-alias postgres
            -e POSTGRES_DB=jacklimited_test
            -e POSTGRES_USER=postgres
            -e POSTGRES_PASSWORD=password
            postgres:15-alpine
        `.replace(/\s+/g, ' '));

        if (!dbResult.success) {
            throw new Error(`Failed to start database: ${dbResult.error}`);
        }

        // Wait for database
        log('blue', 'Waiting for database to be ready...');
        let dbReady = false;
        for (let i = 0; i < CONFIG.maxRetries; i++) {
            const healthResult = DockerManager.execute(
                `docker exec ${this.dbContainerName} pg_isready -U testuser -d jacklimited_test`,
                { silent: true }
            );
            if (healthResult.success) {
                dbReady = true;
                break;
            }
            await new Promise(resolve => setTimeout(resolve, 2000));
        }

        if (!dbReady) {
            throw new Error('Database failed to start within timeout');
        }

        logTest('Database Setup', 'PASS', 'PostgreSQL container ready');
    }

    async buildImage() {
        log('cyan', 'Building Docker image...');

        const buildResult = DockerManager.execute(`docker build -t ${CONFIG.imageName} .`);

        if (!buildResult.success) {
            throw new Error(`Docker build failed: ${buildResult.error}`);
        }

        logTest('Docker Build', 'PASS', 'Image built successfully');
    }

    async startApplication() {
        log('cyan', 'Starting application container...');

        const runResult = DockerManager.execute(`
            docker run -d
            --name ${CONFIG.containerName}
            --network ${this.networkName}
            -e ASPNETCORE_ENVIRONMENT=Testing
            -e ConnectionStrings__DefaultConnection="Host=${this.dbContainerName};Database=jacklimited_test;Username=postgres;Password=password"
            -p 8081:8080
            ${CONFIG.imageName}
        `.replace(/\s+/g, ' '));

        if (!runResult.success) {
            throw new Error(`Failed to start application: ${runResult.error}`);
        }

        // Wait for application to be ready
        log('blue', 'Waiting for application to be ready...');
        const appReady = await DockerManager.waitForHealth(`${CONFIG.apiBaseUrl}/readyz`);

        if (!appReady) {
            const logs = DockerManager.execute(`docker logs ${CONFIG.containerName}`, { silent: true });
            throw new Error(`Application failed to start. Logs: ${logs.output}`);
        }

        logTest('Application Startup', 'PASS', 'Container responding on port 8081');
    }

    async testHealthChecks() {
        log('cyan', 'Testing health checks...');

        const checks = [
            { name: 'Liveness', path: '/healthz' },
            { name: 'Readiness', path: '/readyz', expectReadyTag: true }
        ];

        for (const check of checks) {
            try {
                const response = await fetch(`${CONFIG.apiBaseUrl}${check.path}`);
                if (!response.ok) {
                    logTest(`${check.name} Check`, 'FAIL', `${check.path} returned ${response.status}`);
                    continue;
                }

                const payload = await response.json();
                const statusHealthy = payload?.status === 'Healthy';
                const readyTagPresent = !check.expectReadyTag || payload?.checks?.some(entry => entry.tags?.includes('ready'));

                if (statusHealthy && readyTagPresent) {
                    logTest(`${check.name} Check`, 'PASS', `${check.path} healthy`);
                } else {
                    logTest(`${check.name} Check`, 'FAIL', `Unexpected payload: ${JSON.stringify(payload)}`);
                }
            } catch (error) {
                logTest(`${check.name} Check`, 'FAIL', `Request failed: ${error.message}`);
            }
        }
    }

    async testApiEndpoints() {
        log('cyan', 'Testing API endpoints...');

        const endpoints = [
            {
                path: '/api/survey/nps',
                validate: data => data && typeof data.nps === 'number'
            },
            {
                path: '/api/survey/average',
                validate: data => data && typeof data.average === 'number'
            },
            {
                path: '/api/survey/distribution',
                validate: data => data && typeof data === 'object'
            }
        ];

        for (const endpoint of endpoints) {
            try {
                const response = await fetch(`${CONFIG.apiBaseUrl}${endpoint.path}`);
                const data = await response.json();

                if (response.ok && endpoint.validate(data)) {
                    logTest(`API ${endpoint.path}`, 'PASS', 'Returns valid data');
                } else {
                    logTest(`API ${endpoint.path}`, 'FAIL', `Invalid response: ${JSON.stringify(data)}`);
                }
            } catch (error) {
                logTest(`API ${endpoint.path}`, 'FAIL', `Request failed: ${error.message}`);
            }
        }
    }

    async testSurveySubmission() {
        log('cyan', 'Testing survey submission...');

        try {
            const surveyData = {
                likelihoodToRecommend: 8,
                comment: 'Test survey from TDD suite',
                email: 'test@example.com'
            };

            const response = await fetch(`${CONFIG.apiBaseUrl}/api/survey`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(surveyData)
            });

            const result = await response.json();

            if (response.ok && result.id) {
                logTest('Survey Submission', 'PASS', `Survey created with ID: ${result.id}`);

                // Verify it appears in analytics
                await new Promise(resolve => setTimeout(resolve, 2000)); // Wait for processing

                const npsResponse = await fetch(`${CONFIG.apiBaseUrl}/api/survey/nps`);
                const npsData = await npsResponse.json();

                if (npsData.nps !== null && npsData.nps !== undefined) {
                    logTest('Survey Analytics', 'PASS', 'Survey data reflected in analytics');
                } else {
                    logTest('Survey Analytics', 'FAIL', 'Survey data not found in analytics');
                }
            } else {
                logTest('Survey Submission', 'FAIL', `Submission failed: ${JSON.stringify(result)}`);
            }
        } catch (error) {
            logTest('Survey Submission', 'FAIL', `Test failed: ${error.message}`);
        }
    }

    async testFrontendServing() {
        log('cyan', 'Testing frontend serving...');

        try {
            const response = await fetch(CONFIG.apiBaseUrl);
            if (response.ok && response.headers.get('content-type')?.includes('text/html')) {
                logTest('Frontend Serving', 'PASS', 'HTML page served correctly');
            } else {
                logTest('Frontend Serving', 'FAIL', `Unexpected response: ${response.status}`);
            }
        } catch (error) {
            logTest('Frontend Serving', 'FAIL', `Frontend test failed: ${error.message}`);
        }
    }

    async testErrorHandling() {
        log('cyan', 'Testing error handling...');

        // Test invalid JSON
        try {
            const response = await fetch(`${CONFIG.apiBaseUrl}/api/survey`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: '{"invalid": json'
            });

            if (response.status === 400) {
                logTest('Error Handling - Invalid JSON', 'PASS', 'Properly handled malformed JSON');
            } else {
                logTest('Error Handling - Invalid JSON', 'FAIL', `Unexpected status: ${response.status}`);
            }
        } catch (error) {
            logTest('Error Handling - Invalid JSON', 'FAIL', `Test failed: ${error.message}`);
        }

        // Test missing required fields
        try {
            const response = await fetch(`${CONFIG.apiBaseUrl}/api/survey`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ likelihoodToRecommend: 11 })
            });

            const result = await response.json();

            if (response.status === 400 && result.errors) {
                logTest('Error Handling - Validation', 'PASS', 'Properly validated out-of-range rating');
            } else {
                logTest('Error Handling - Validation', 'FAIL', `Unexpected response: ${response.status}`);
            }
        } catch (error) {
            logTest('Error Handling - Validation', 'FAIL', `Test failed: ${error.message}`);
        }
    }

    async testPerformance() {
        log('cyan', 'Testing performance...');

        const startTime = Date.now();

        try {
            await fetch(`${CONFIG.apiBaseUrl}/api/survey/nps`);
            const endTime = Date.now();
            const responseTime = endTime - startTime;

            if (responseTime < 1000) {
                logTest('Performance', 'PASS', `Response time: ${responseTime}ms`);
            } else {
                logTest('Performance', 'FAIL', `Response too slow: ${responseTime}ms`);
            }
        } catch (error) {
            logTest('Performance', 'FAIL', `Performance test failed: ${error.message}`);
        }
    }

    async testSecurity() {
        log('cyan', 'Testing security basics...');

        // Test testing endpoints require authorization
        try {
            const unauthorized = await fetch(`${CONFIG.apiBaseUrl}/testing/reset`, {
                method: 'POST'
            });
            if (unauthorized.status === 401) {
                logTest('Security - Testing Endpoints (Unauthorized)', 'PASS', 'Reset endpoint requires auth');
            } else {
                logTest('Security - Testing Endpoints (Unauthorized)', 'FAIL', `Unexpected status: ${unauthorized.status}`);
            }

            const authorized = await fetch(`${CONFIG.apiBaseUrl}/testing/reset`, {
                method: 'POST',
                headers: { 'X-Test-Auth': 'local-testing-key' }
            });
            if (authorized.ok) {
                logTest('Security - Testing Endpoints (Authorized)', 'PASS', 'Authorized access succeeds');
            } else {
                logTest('Security - Testing Endpoints (Authorized)', 'FAIL', `Authorized request failed: ${authorized.status}`);
            }
        } catch (error) {
            logTest('Security - Testing Endpoints', 'FAIL', `Testing endpoint check failed: ${error.message}`);
        }

        // Test restrictive CORS policy
        try {
            const evilResponse = await fetch(`${CONFIG.apiBaseUrl}/api/survey/nps`, {
                headers: { 'Origin': 'http://evil.com' }
            });
            const evilHeader = evilResponse.headers.get('access-control-allow-origin');
            if (!evilHeader) {
                logTest('Security - CORS (Denied Origin)', 'PASS', 'Disallowed origin not echoed');
            } else {
                logTest('Security - CORS (Denied Origin)', 'FAIL', `Unexpected header: ${evilHeader}`);
            }

            const allowedResponse = await fetch(`${CONFIG.apiBaseUrl}/api/survey/nps`, {
                headers: { 'Origin': 'http://localhost:5173' }
            });
            const allowedHeader = allowedResponse.headers.get('access-control-allow-origin');
            if (allowedHeader === 'http://localhost:5173') {
                logTest('Security - CORS (Allowed Origin)', 'PASS', 'Allowed origin echoed correctly');
            } else {
                logTest('Security - CORS (Allowed Origin)', 'FAIL', 'Allowed origin header missing');
            }
        } catch (error) {
            logTest('Security - CORS', 'FAIL', `CORS test failed: ${error.message}`);
        }
    }

    async cleanup() {
        log('cyan', 'Cleaning up test environment...');

        DockerManager.execute(`docker stop ${CONFIG.containerName}`, { silent: true });
        DockerManager.execute(`docker stop ${this.dbContainerName}`, { silent: true });
        DockerManager.execute(`docker rm ${CONFIG.containerName}`, { silent: true });
        DockerManager.execute(`docker rm ${this.dbContainerName}`, { silent: true });
        DockerManager.execute(`docker network rm ${this.networkName}`, { silent: true });

        log('green', 'Cleanup completed');
    }

    async runAllTests() {
        let exitCode = 0;

        try {
            log('magenta', '🚀 Starting TDD/BDD Docker Container Test Suite');
            log('magenta', 'Following Test-Driven Development and Behavior-Driven Development principles');

            await this.setup();
            await this.buildImage();
            await this.startApplication();
            await this.testHealthChecks();
            await this.testApiEndpoints();
            await this.testSurveySubmission();
            await this.testFrontendServing();
            await this.testErrorHandling();
            await this.testPerformance();
            await this.testSecurity();

            // Summary
            log('magenta', '\n📊 Test Results Summary:');
            log('blue', `Total Tests: ${testResults.total}`);
            log('green', `Passed: ${testResults.passed}`);
            log('red', `Failed: ${testResults.failed}`);
            log('yellow', `Skipped: ${testResults.skipped}`);

            if (testResults.failed === 0) {
                log('green', '🎉 All tests passed! Docker image is ready for deployment.');
            } else {
                log('red', '❌ Some tests failed. Please review and fix issues before deployment.');
                exitCode = 1;
            }

        } catch (error) {
            log('red', `💥 Test suite failed: ${error.message}`);
            exitCode = 1;
        } finally {
            await this.cleanup();
            process.exit(exitCode);
        }
    }
}

// CLI interface
async function main() {
    const args = process.argv.slice(2);
    const testSuite = new DockerTestSuite();

    if (args.includes('--help') || args.includes('-h')) {
        console.log(`
Docker Container Test Suite (TDD/BDD)
Usage: node test-docker-container.js [options]

Options:
  --setup-only     Only setup environment, don't run tests
  --build-only     Only build image, don't run tests
  --api-only       Only run API tests
  --integration    Run integration tests only
  --performance    Run performance tests only
  --security       Run security tests only
  --help, -h       Show this help

Examples:
  node test-docker-container.js                    # Run all tests
  node test-docker-container.js --api-only        # Test APIs only
  node test-docker-container.js --performance     # Performance tests only
        `);
        return;
    }

    if (args.includes('--setup-only')) {
        await testSuite.setup();
        return;
    }

    if (args.includes('--build-only')) {
        await testSuite.setup();
        await testSuite.buildImage();
        return;
    }

    // Run specific test categories or all tests
    if (args.includes('--api-only')) {
        await testSuite.setup();
        await testSuite.buildImage();
        await testSuite.startApplication();
        await testSuite.testApiEndpoints();
    } else if (args.includes('--integration')) {
        await testSuite.setup();
        await testSuite.buildImage();
        await testSuite.startApplication();
        await testSuite.testSurveySubmission();
        await testSuite.testFrontendServing();
    } else if (args.includes('--performance')) {
        await testSuite.setup();
        await testSuite.buildImage();
        await testSuite.startApplication();
        await testSuite.testPerformance();
    } else if (args.includes('--security')) {
        await testSuite.setup();
        await testSuite.buildImage();
        await testSuite.startApplication();
        await testSuite.testSecurity();
    } else {
        // Run all tests
        await testSuite.runAllTests();
    }
}

if (require.main === module) {
    main().catch(error => {
        console.error('Fatal error:', error);
        process.exit(1);
    });
}

module.exports = { DockerTestSuite, DockerManager };