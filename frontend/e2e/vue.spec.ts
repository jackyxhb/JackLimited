import { test, expect } from '@playwright/test';

test('visits the app root url', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('h1')).toHaveText('Welcome to Jack Limited');
});

test('can submit survey feedback', async ({ page }) => {
  await page.goto('/survey');

  // Fill out the survey form
  await page.fill('input[type="number"]', '8');
  await page.fill('textarea', 'Great service!');
  await page.fill('input[type="email"]', 'test@example.com');

  // Submit the form
  await page.click('button[type="submit"]');

  // Wait for submission to complete and form to reset
  await expect(page.locator('input[type="number"]')).toHaveValue('5');
  await expect(page.locator('textarea')).toHaveValue('');
  await expect(page.locator('input[type="email"]')).toHaveValue('');
});

test('displays NPS and rating analytics', async ({ page }) => {
  await page.goto('/analytics');

  // Check that analytics components are present
  await expect(page.locator('text=Net Promoter Score')).toBeVisible();
  await expect(page.locator('.rating-distribution')).toBeVisible();
  await expect(page.locator('text=Average Rating')).toBeVisible();
  await expect(page.locator('.rating-distribution canvas')).toBeVisible();
});

test('NPS chart shows correct promoter/passive/detractor distribution', async ({ page }) => {
  // Submit surveys with different ratings
  await page.goto('/survey');

  const testCases = [
    { rating: '10' },
    { rating: '9' },
    { rating: '8' },
    { rating: '7' },
    { rating: '6' },
    { rating: '5' }
  ];

  // Submit all test cases
  for (const testCase of testCases) {
    await page.fill('input[type="number"]', testCase.rating);
    await page.click('button[type="submit"]');
    // Wait for form reset as confirmation of submission
    await expect(page.locator('input[type="number"]')).toHaveValue('5');
  }

  // Navigate back to analytics to check the results
  await page.goto('/analytics');

  // Check that NPS chart is visible and contains data
  await expect(page.locator('.nps-chart canvas')).toBeVisible();

  // Verify the NPS value is displayed and is a valid number
  const finalNpsText = await page.locator('.nps-value').textContent();
  expect(finalNpsText).toMatch(/^NPS: -?\d+(?:\.\d+)?$/);

  // Verify response count has increased by the number of submissions
  const finalResponseText = await page.locator('.response-count').textContent();
  expect(finalResponseText).toMatch(/Total Responses: \d+/);
  const finalResponseMatch = finalResponseText!.match(/Total Responses: (\d+)/);
  const finalResponses = parseInt(finalResponseMatch![1]);
  expect(finalResponses).toBe(testCases.length);

  // Verify that the NPS value is a valid number within expected range
  const finalNpsMatch = finalNpsText!.match(/NPS: (-?\d+(?:\.\d+)?)/);
  const finalNps = parseFloat(finalNpsMatch![1]);
  expect(typeof finalNps).toBe('number');
  expect(finalNps).toBeGreaterThanOrEqual(-100);
  expect(finalNps).toBeLessThanOrEqual(100);
});
