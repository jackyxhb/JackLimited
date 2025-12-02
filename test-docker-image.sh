#!/bin/bash

# Docker Image Comprehensive Test Suite
# Following TDD/BDD principles to ensure containerized application quality

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
IMAGE_NAME="jacklimited:test"
CONTAINER_NAME="jacklimited-test-$(date +%s)"
DB_CONTAINER_NAME="${CONTAINER_NAME}-db"
TEST_RESULTS_DIR="test-results/docker"
API_BASE_URL="http://localhost:8081"

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Cleanup function
cleanup() {
    log_info "Cleaning up test containers..."
    docker stop "$CONTAINER_NAME" 2>/dev/null || true
    docker stop "$DB_CONTAINER_NAME" 2>/dev/null || true
    docker rm "$CONTAINER_NAME" 2>/dev/null || true
    docker rm "$DB_CONTAINER_NAME" 2>/dev/null || true
    docker network rm "${CONTAINER_NAME}-network" 2>/dev/null || true
}

# Setup test environment
setup_test_environment() {
    log_info "Setting up test environment..."

    # Create test network
    docker network create "${CONTAINER_NAME}-network"

    # Start PostgreSQL database
    log_info "Starting PostgreSQL database..."
    docker run -d \
        --name "$DB_CONTAINER_NAME" \
        --network "${CONTAINER_NAME}-network" \
        --network-alias postgres \
        -e POSTGRES_DB=jacklimited_test \
        -e POSTGRES_USER=postgres \
        -e POSTGRES_PASSWORD=password \
        -p 5433:5432 \
        postgres:15-alpine

    # Wait for database to be ready
    log_info "Waiting for database to be ready..."
    for i in {1..30}; do
        if docker exec "$DB_CONTAINER_NAME" pg_isready -U postgres -d jacklimited_test >/dev/null 2>&1; then
            log_success "Database is ready"
            break
        fi
        sleep 2
    done

    if [ $i -eq 30 ]; then
        log_error "Database failed to start"
        exit 1
    fi
}

# Build test image
build_test_image() {
    log_info "Building test Docker image..."
    docker build -t "$IMAGE_NAME" .

    if [ $? -eq 0 ]; then
        log_success "Docker image built successfully"
    else
        log_error "Docker image build failed"
        exit 1
    fi
}

# Test container startup (TDD: Container should start successfully)
test_container_startup() {
    log_info "Testing container startup (TDD)..."

    # Start the application container
    docker run -d \
        --name "$CONTAINER_NAME" \
        --network "${CONTAINER_NAME}-network" \
        -e ASPNETCORE_ENVIRONMENT=Testing \
        -e ConnectionStrings__DefaultConnection="Host=$DB_CONTAINER_NAME;Database=jacklimited_test;Username=postgres;Password=password" \
        -p 8081:8080 \
        "$IMAGE_NAME"

    # Wait for container to start
    log_info "Waiting for application to start..."
    for i in {1..30}; do
        if curl -s "$API_BASE_URL" >/dev/null 2>&1; then
            log_success "Application is responding"
            break
        fi
        sleep 2
    done

    if [ $i -eq 30 ]; then
        log_error "Application failed to start"
        docker logs "$CONTAINER_NAME"
        exit 1
    fi
}

# Test health checks (BDD: As an operator, I want proper health monitoring)
test_health_checks() {
    log_info "Testing health checks (BDD)..."

    # Test basic health endpoint
    response=$(curl -s -o /dev/null -w "%{http_code}" "$API_BASE_URL/health" 2>/dev/null || echo "000")
    if [ "$response" = "200" ]; then
        log_success "Health check endpoint responds correctly"
    else
        log_warning "Health check endpoint returned $response (expected 200)"
    fi

    # Test database connectivity
    response=$(curl -s "$API_BASE_URL/api/survey/nps" 2>/dev/null || echo "error")
    if [ "$response" != "error" ]; then
        log_success "Database connectivity test passed"
    else
        log_error "Database connectivity test failed"
        exit 1
    fi
}

# Test API endpoints (TDD: All endpoints should work in containerized environment)
test_api_endpoints() {
    log_info "Testing API endpoints (TDD)..."

    # Test NPS endpoint
    response=$(curl -s -X GET "$API_BASE_URL/api/survey/nps")
    if echo "$response" | jq -e '.nps' >/dev/null 2>&1; then
        log_success "NPS endpoint returns valid JSON"
    else
        log_error "NPS endpoint failed: $response"
        exit 1
    fi

    # Test average endpoint
    response=$(curl -s -X GET "$API_BASE_URL/api/survey/average")
    if echo "$response" | jq -e '.average' >/dev/null 2>&1; then
        log_success "Average endpoint returns valid JSON"
    else
        log_error "Average endpoint failed: $response"
        exit 1
    fi

    # Test distribution endpoint
    response=$(curl -s -X GET "$API_BASE_URL/api/survey/distribution")
    if echo "$response" | jq -e 'type == "object"' >/dev/null 2>&1; then
        log_success "Distribution endpoint returns valid JSON object"
    else
        log_error "Distribution endpoint failed: $response"
        exit 1
    fi
}

# Test survey submission (BDD: As a user, I want to submit feedback successfully)
test_survey_submission() {
    log_info "Testing survey submission (BDD)..."

    # Submit a test survey
    response=$(curl -s -X POST "$API_BASE_URL/api/survey" \
        -H "Content-Type: application/json" \
        -d '{
            "likelihoodToRecommend": 8,
            "comment": "Test survey from Docker test suite",
            "email": "test@example.com"
        }')

    if echo "$response" | jq -e '.id' >/dev/null 2>&1; then
        log_success "Survey submission successful"
        SURVEY_ID=$(echo "$response" | jq -r '.id')
    else
        log_error "Survey submission failed: $response"
        exit 1
    fi

    # Verify the survey appears in analytics
    sleep 2
        response=$(curl -s -X GET "$API_BASE_URL/api/survey/distribution")
        rating_count=$(echo "$response" | jq -r '."8" // 0')

        if [ "$rating_count" -ge 1 ]; then
        log_success "Survey data appears in analytics"
    else
        log_error "Survey data not reflected in analytics"
        exit 1
    fi
}

# Test frontend serving (TDD: Static files should be served correctly)
test_frontend_serving() {
    log_info "Testing frontend serving (TDD)..."

    # Test main HTML file
    response=$(curl -s -o /dev/null -w "%{http_code}" "$API_BASE_URL/" 2>/dev/null || echo "000")
    if [ "$response" = "200" ]; then
        log_success "Main HTML page serves correctly"
    else
        log_error "Main HTML page failed with status $response"
        exit 1
    fi

    # Test static assets
    response=$(curl -s -o /dev/null -w "%{http_code}" "$API_BASE_URL/assets/index.js" 2>/dev/null || echo "000")
    if [ "$response" = "200" ]; then
        log_success "Static assets serve correctly"
    else
        log_warning "Static assets check returned $response (may be expected for SPA routing)"
    fi
}

# Test database persistence (BDD: As a user, I want my data to persist)
test_database_persistence() {
    log_info "Testing database persistence (BDD)..."

    # Reset database first
    reset_response=$(curl -s -X POST "$API_BASE_URL/testing/reset" \
        -H "X-Test-Auth: local-testing-key")
    if echo "$reset_response" | jq -e '.message' >/dev/null 2>&1; then
        log_info "Database reset successfully"
    else
        log_warning "Database reset failed: $reset_response"
    fi

    # Get initial count
    initial_response=$(curl -s -X GET "$API_BASE_URL/api/survey/distribution")
        initial_count=$(echo "$initial_response" | jq '[.[]] | add // 0')

    # Submit a survey
    submit_response=$(curl -s -X POST "$API_BASE_URL/api/survey" \
        -H "Content-Type: application/json" \
        -d '{
            "likelihoodToRecommend": 9,
            "comment": "Persistence test survey",
            "email": "persist@example.com"
        }')

    if echo "$submit_response" | jq -e '.id' >/dev/null 2>&1; then
        log_info "Survey submitted successfully"
    else
        log_error "Survey submission failed: $submit_response"
        exit 1
    fi

    # Check count increased
    final_response=$(curl -s -X GET "$API_BASE_URL/api/survey/distribution")
        final_count=$(echo "$final_response" | jq '[.[]] | add // 0')

    if [ "$final_count" -gt "$initial_count" ]; then
        log_success "Database persistence working correctly"
    else
        log_error "Database persistence failed - count didn't increase"
        exit 1
    fi
}

# Test error handling (TDD: Application should handle errors gracefully)
test_error_handling() {
    log_info "Testing error handling (TDD)..."

    # Test invalid JSON
    response=$(curl -s -X POST "$API_BASE_URL/api/survey" \
        -H "Content-Type: application/json" \
        -d '{"invalid": json}')

    if echo "$response" | jq -e '.message' >/dev/null 2>&1; then
        log_info "Invalid JSON handled gracefully"
    else
        log_warning "Invalid JSON response: $response"
    fi

    # Test missing required fields
    response=$(curl -s -X POST "$API_BASE_URL/api/survey" \
        -H "Content-Type: application/json" \
        -d '{}')

    if echo "$response" | jq -e '.message' >/dev/null 2>&1; then
        log_info "Missing fields handled gracefully"
    else
        log_warning "Missing fields response: $response"
    fi
}

# Test performance (BDD: As a user, I want reasonable response times)
test_performance() {
    log_info "Testing performance (BDD)..."

    # Test API response times
    start_time=$(date +%s%N)
    curl -s "$API_BASE_URL/api/survey/nps" >/dev/null
    end_time=$(date +%s%N)
    response_time=$(( (end_time - start_time) / 1000000 )) # Convert to milliseconds

    if [ "$response_time" -lt 1000 ]; then # Less than 1 second
        log_success "API response time acceptable: ${response_time}ms"
    else
        log_warning "API response time slow: ${response_time}ms"
    fi
}

# Test security basics (TDD: Basic security measures should be in place)
test_security_basics() {
    log_info "Testing security basics (TDD)..."

    # Test CORS headers
    response=$(curl -s -I -H "Origin: http://evil.com" "$API_BASE_URL/api/survey/nps" 2>/dev/null | grep -i "access-control-allow-origin" || echo "none")
    if [ "$response" != "none" ]; then
        log_success "CORS headers present"
    else
        log_warning "CORS headers missing"
    fi

    # Test that API endpoints require proper content type
    response=$(curl -s -o /dev/null -w "%{http_code}" "$API_BASE_URL/api/survey" \
        -X POST \
        -d "rating=9&comment=test" 2>/dev/null || echo "400")
    if [ "$response" = "400" ]; then
        log_success "Content-Type validation working"
    else
        log_warning "Content-Type validation may be missing"
    fi
}

# Run all tests
run_all_tests() {
    log_info "Starting comprehensive Docker image test suite..."
    log_info "Following TDD/BDD principles for container validation"

    mkdir -p "$TEST_RESULTS_DIR"

    # Setup
    setup_test_environment
    build_test_image

    # Test execution
    test_container_startup
    test_health_checks
    test_api_endpoints
    test_survey_submission
    test_frontend_serving
    test_database_persistence
    test_error_handling
    test_performance
    test_security_basics

    log_success "All Docker image tests completed successfully!"
    log_info "Containerized application is ready for deployment"
}

# Main execution
trap cleanup EXIT

case "${1:-all}" in
    "startup")
        setup_test_environment
        build_test_image
        test_container_startup
        ;;
    "api")
        setup_test_environment
        build_test_image
        test_container_startup
        test_api_endpoints
        ;;
    "integration")
        setup_test_environment
        build_test_image
        test_container_startup
        test_survey_submission
        test_database_persistence
        ;;
    "all")
        run_all_tests
        ;;
    *)
        echo "Usage: $0 [startup|api|integration|all]"
        echo "  startup    - Test container startup only"
        echo "  api        - Test API endpoints"
        echo "  integration - Test full integration scenarios"
        echo "  all        - Run complete test suite (default)"
        exit 1
        ;;
esac