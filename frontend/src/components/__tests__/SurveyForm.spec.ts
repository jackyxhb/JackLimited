import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import SurveyForm from '../SurveyForm.vue'

const submitSurveyMock = vi.fn()
const toastSuccessMock = vi.fn()
const toastErrorMock = vi.fn()

vi.mock('@/stores/survey', () => ({
  useSurveyStore: () => ({
    submitSurvey: submitSurveyMock,
  }),
}))

vi.mock('@/stores/toast', () => ({
  useToastStore: () => ({
    success: toastSuccessMock,
    error: toastErrorMock,
  }),
}))

vi.mock('lucide-vue-next', () => {
  const stub = { template: '<span />' }
  return {
    SendIcon: stub,
    StarIcon: stub,
    MessageSquareIcon: stub,
    UsersIcon: stub,
    AlertCircleIcon: stub,
    RotateCcwIcon: stub,
  }
})

describe('SurveyForm', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    submitSurveyMock.mockResolvedValue({ ok: true })
  })

  it('prevents submission when rating is outside the allowed range', async () => {
    const wrapper = mount(SurveyForm)

    await wrapper.find('#rating').setValue(11)
    await wrapper.find('form').trigger('submit.prevent')

    expect(wrapper.text()).toContain('Rating must be between 0 and 10')
    expect(submitSurveyMock).not.toHaveBeenCalled()
  })

  it('sanitizes user input and shows a success toast on submit', async () => {
    const wrapper = mount(SurveyForm)

    await wrapper.find('#rating').setValue(9)
    await wrapper.find('#comments').setValue('  <script>alert(1)</script>  ')
    await wrapper.find('#email').setValue(' user@example.com ')
    await wrapper.find('form').trigger('submit.prevent')
    await flushPromises()

    expect(submitSurveyMock).toHaveBeenCalledTimes(1)
    const payload = submitSurveyMock.mock.calls[0][0]
    expect(payload.likelihoodToRecommend).toBe(9)
    expect(payload.comments).toBeDefined()
    expect(payload.comments).not.toMatch(/[<>]/)
    expect(payload.email).toBe('user@example.com')
    expect(toastSuccessMock).toHaveBeenCalledWith(
      'Feedback Submitted!',
      'Thank you for your valuable feedback.',
      3000
    )
  })

  it('surfaces a toast error when submission fails', async () => {
    submitSurveyMock.mockRejectedValueOnce(new Error('500: failure'))
    const wrapper = mount(SurveyForm)

    await wrapper.find('#rating').setValue(6)
    await wrapper.find('form').trigger('submit.prevent')
    await flushPromises()

    expect(toastErrorMock).toHaveBeenCalled()
  })
})
