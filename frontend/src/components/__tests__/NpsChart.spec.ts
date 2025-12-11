import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import NpsChart from '../NpsChart.vue'

const chartStub = vi.hoisted(() => ({
  name: 'Doughnut',
  props: {
    data: { type: Object, required: true },
    options: { type: Object, required: true },
  },
  template: '<div class="chart-stub" />'
}))

vi.mock('vue-chartjs', () => ({
  Doughnut: chartStub,
}))

describe('NpsChart', () => {
  const baseDistribution = {
    10: 2,
    9: 1,
    8: 1,
    6: 1,
  }

  const mountChart = (overrides = {}) =>
    mount(NpsChart, {
      props: {
        nps: 25,
        distribution: baseDistribution,
        ...overrides,
      },
    })

  it('renders loading state', () => {
    const wrapper = mountChart({ isLoading: true })
    expect(wrapper.text()).toContain('Loading NPS data')
    expect(wrapper.find('.loading-spinner').exists()).toBe(true)
  })

  it('renders error state and triggers retry handler', async () => {
    const onRetry = vi.fn()
    const wrapper = mountChart({ error: 'Failed to load data', onRetry })

    expect(wrapper.text()).toContain('Failed to load data')
    await wrapper.find('button.retry-button').trigger('click')
    expect(onRetry).toHaveBeenCalledTimes(1)
  })

  it('computes total responses and passes grouped data to Doughnut chart', () => {
    const wrapper = mountChart()

    expect(wrapper.text()).toContain('Total Responses: 5')
    expect(wrapper.text()).toContain('NPS: 25')

    const doughnut = wrapper.findComponent(chartStub)
    const dataProp = doughnut.props('data') as { datasets: Array<{ data: number[] }> }

    expect(dataProp.labels).toEqual([
      'Promoters (9-10)',
      'Passives (7-8)',
      'Detractors (0-6)'
    ])
    expect(dataProp.datasets[0].data).toEqual([3, 1, 1])
  })
})
