Feature: JackLimited Docker Container
  As a DevOps engineer
  I want the JackLimited application to run reliably in Docker containers
  So that I can deploy it consistently across environments

  Background:
    Given the Docker image is built successfully
    And the PostgreSQL database is running
    And the application container is started with proper environment variables

  @startup @tdd
  Scenario: Container starts successfully
    When the Docker container is started
    Then the application should be listening on port 8080
    And the health check endpoint should respond with HTTP 200
    And the main application endpoint should be accessible

  @api @tdd
  Scenario: API endpoints work correctly
    When I make a GET request to "/api/survey/nps"
    Then I should receive a valid JSON response
    And the response should contain an "nps" field

    When I make a GET request to "/api/survey/average"
    Then I should receive a valid JSON response
    And the response should contain an "average" field

    When I make a GET request to "/api/survey/distribution"
    Then I should receive a valid JSON response
    And the response should contain a "distribution" object

  @frontend @bdd
  Scenario: Frontend serves static files
    Given I am a user accessing the web application
    When I navigate to the root URL "/"
    Then I should receive the main HTML page
    And static assets should be served correctly
    And the SPA should load without JavaScript errors

  @database @bdd
  Scenario: Database connectivity works
    Given I am a user submitting survey data
    When I submit a survey with rating 8 and comment "Great service"
    Then the survey should be stored in the database
    And the survey data should appear in analytics endpoints
    And the data should persist across container restarts

  @analytics @bdd
  Scenario: Analytics calculations work correctly
    Given multiple surveys have been submitted
    When I view the analytics dashboard
    Then the NPS score should be calculated correctly
    And the average rating should be computed accurately
    And the rating distribution should show all ratings 1-10
    And charts should render with proper data visualization

  @error-handling @tdd
  Scenario: Error handling works properly
    When I send invalid JSON to the survey endpoint
    Then I should receive a 400 Bad Request response
    And the error should be logged appropriately

    When I submit a survey with missing required fields
    Then I should receive validation error messages
    And the response should indicate which fields are required

  @performance @bdd
  Scenario: Application performs adequately
    Given the application is running in a container
    When I make multiple API requests
    Then response times should be under 1 second
    And memory usage should be within acceptable limits
    And CPU usage should not exceed normal thresholds

  @security @tdd
  Scenario: Security measures are in place
    Given the application is running in production mode
    When I attempt to access testing endpoints
    Then I should receive a 404 Not Found response

    When I make requests from different origins
    Then CORS headers should be properly configured
    And sensitive endpoints should require authentication

  @health-monitoring @bdd
  Scenario: Health monitoring works
    Given I am a system operator
    When I check the health endpoint
    Then I should receive current system status
    And database connectivity should be verified
    And any issues should be clearly reported

  @theme @bdd
  Scenario: Dark mode works in containerized environment
    Given I am a user with dark mode preference
    When I access the analytics dashboard
    Then the rating distribution chart should adapt to dark theme
    And text colors should be appropriate for dark background
    And all UI elements should be visible and readable

  @persistence @bdd
  Scenario: Data persistence across deployments
    Given surveys have been submitted to the database
    When the container is restarted
    Then all survey data should still be available
    And analytics calculations should remain accurate
    And no data loss should occur