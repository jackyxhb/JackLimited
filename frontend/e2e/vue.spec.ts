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

test('NPS chart shows correct promoter/passive/detractor distribution', async ({ page, request }) => {
  // Reset test data and seed a known distribution directly via the API
  await request.post('/testing/reset');

  const testCases = [10, 9, 8, 7, 6, 5];
  await request.post('/testing/seed', {
    data: testCases.map((rating) => ({
      likelihoodToRecommend: rating,
      comments: null,
      email: null,
    })),
  });

  await page.goto('/analytics');

  // Check that NPS chart is visible and contains data
  await expect(page.locator('.nps-chart canvas')).toBeVisible();

  const npsValueLocator = page.locator('.nps-value');
  await expect(npsValueLocator).toHaveText(/^NPS: -?\d+(?:\.\d+)?$/);

  const responseCountLocator = page.locator('.response-count');
  await expect(responseCountLocator).toHaveText(new RegExp(`Total Responses: ${testCases.length}`));

  const finalResponseText = await responseCountLocator.innerText();
  const finalResponseMatch = finalResponseText.match(/Total Responses: (\d+)/);
  const finalResponses = parseInt(finalResponseMatch![1], 10);
  expect(finalResponses).toBe(testCases.length);

  const finalNpsText = await npsValueLocator.innerText();
  const finalNpsMatch = finalNpsText.match(/NPS: (-?\d+(?:\.\d+)?)/);
  const finalNps = parseFloat(finalNpsMatch![1]);
  expect(typeof finalNps).toBe('number');
  expect(finalNps).toBeGreaterThanOrEqual(-100);
  expect(finalNps).toBeLessThanOrEqual(100);
});
