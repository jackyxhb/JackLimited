import { test, expect } from '@playwright/test';

test('visits the app root url', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('h1')).toHaveText('Jack Limited Feedback Portal');
});

test('can submit survey feedback', async ({ page }) => {
  await page.goto('/');

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
  await page.goto('/');

  // Wait for the page to load and data to be fetched
  await page.waitForTimeout(2000);

  // Check that analytics components are present
  await expect(page.locator('text=Net Promoter Score')).toBeVisible();
  await expect(page.locator('.rating-distribution')).toBeVisible();
  await expect(page.locator('text=Average Rating')).toBeVisible();
  await expect(page.locator('.rating-distribution canvas')).toBeVisible();
});

test('NPS chart shows correct promoter/passive/detractor distribution', async ({ page }) => {
  await page.goto('/');

  // Submit surveys with different ratings to test NPS categorization
  const testCases = [
    { rating: '10', expected: 'promoter' }, // Promoter (9-10)
    { rating: '9', expected: 'promoter' },  // Promoter (9-10)
    { rating: '8', expected: 'passive' },   // Passive (7-8)
    { rating: '7', expected: 'passive' },   // Passive (7-8)
    { rating: '6', expected: 'detractor' }, // Detractor (0-6)
    { rating: '5', expected: 'detractor' }, // Detractor (0-6)
  ];

  for (const testCase of testCases) {
    await page.fill('input[type="number"]', testCase.rating);
    await page.click('button[type="submit"]');
    await page.waitForTimeout(500); // Wait for submission
  }

  // Wait for analytics to update
  await page.waitForTimeout(2000);

  // Check that NPS chart is visible and contains data
  await expect(page.locator('.nps-chart canvas')).toBeVisible();

  // The NPS value should be calculated correctly
  // With 2 promoters, 2 passives, 2 detractors: NPS = (2-2)/6 * 100 = 0
  const npsText = await page.locator('.nps-value').textContent();
  expect(npsText).toContain('NPS: 0');
});
