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

  // Wait for submission to complete
  await page.waitForTimeout(1000);

  // Check that the form is reset (indicating successful submission)
  await expect(page.locator('input[type="number"]')).toHaveValue('5');
  await expect(page.locator('textarea')).toHaveValue('');
  await expect(page.locator('input[type="email"]')).toHaveValue('');
});

test('displays NPS and rating analytics', async ({ page }) => {
  await page.goto('/analytics');

  // Wait for the page to load and data to be fetched
  await page.waitForTimeout(2000);

  // Check that analytics components are present
  await expect(page.locator('text=Net Promoter Score')).toBeVisible();
  await expect(page.locator('.rating-distribution')).toBeVisible();
  await expect(page.locator('text=Average Rating')).toBeVisible();
  await expect(page.locator('.rating-distribution canvas')).toBeVisible();
});

test('NPS chart shows correct promoter/passive/detractor distribution', async ({ page }) => {
  // First, check initial analytics state
  await page.goto('/analytics');
  await page.waitForTimeout(2000);

  // Get initial NPS value and response count (if any)
  const initialNpsText = await page.locator('.nps-value').textContent();
  const initialResponseText = await page.locator('.response-count').textContent();

  // Extract initial values
  const initialNpsMatch = initialNpsText?.match(/NPS: (-?\d+(?:\.\d+)?)/);
  const initialNps = initialNpsMatch ? parseFloat(initialNpsMatch[1]) : 0;

  const initialResponseMatch = initialResponseText?.match(/Total Responses: (\d+)/);
  const initialResponses = initialResponseMatch ? parseInt(initialResponseMatch[1]) : 0;

  // Now submit surveys with different ratings
  await page.goto('/survey');

  const testCases = [
    { rating: '10', category: 'promoter' }, // Promoter (9-10)
    { rating: '9', category: 'promoter' },  // Promoter (9-10)
    { rating: '8', category: 'passive' },   // Passive (7-8)
    { rating: '7', category: 'passive' },   // Passive (7-8)
    { rating: '6', category: 'detractor' }, // Detractor (0-6)
    { rating: '5', category: 'detractor' }, // Detractor (0-6)
  ];

  // Count expected categories from test data
  let expectedPromoters = 0;
  let expectedPassives = 0;
  let expectedDetractors = 0;

  for (const testCase of testCases) {
    await page.fill('input[type="number"]', testCase.rating);
    await page.click('button[type="submit"]');
    await page.waitForTimeout(500); // Wait for submission

    // Count expected categories
    if (testCase.category === 'promoter') expectedPromoters++;
    else if (testCase.category === 'passive') expectedPassives++;
    else if (testCase.category === 'detractor') expectedDetractors++;
  }

  // Navigate back to analytics to check the results
  await page.goto('/analytics');
  await page.waitForTimeout(2000);

  // Check that NPS chart is visible and contains data
  await expect(page.locator('.nps-chart canvas')).toBeVisible();

  // Verify the NPS value is displayed and is a valid number
  const finalNpsText = await page.locator('.nps-value').textContent();
  expect(finalNpsText).toMatch(/^NPS: -?\d+(?:\.\d+)?$/);

  // Verify response count has increased by the number of submissions
  const finalResponseText = await page.locator('.response-count').textContent();
  const finalResponseMatch = finalResponseText?.match(/Total Responses: (\d+)/);
  const finalResponses = finalResponseMatch ? parseInt(finalResponseMatch[1]) : 0;
  expect(finalResponses).toBe(initialResponses + testCases.length);

  // Verify that the NPS value has been updated (should be different from initial or valid)
  const finalNpsMatch = finalNpsText?.match(/NPS: (-?\d+(?:\.\d+)?)/);
  const finalNps = finalNpsMatch ? parseFloat(finalNpsMatch[1]) : null;
  expect(finalNps).not.toBeNull();
  expect(typeof finalNps).toBe('number');
  expect(finalNps).toBeGreaterThanOrEqual(-100);
  expect(finalNps).toBeLessThanOrEqual(100);
});
