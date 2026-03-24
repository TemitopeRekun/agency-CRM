import { describe, it, expect, vi, beforeEach } from 'vitest'
import { apiRequest } from '../lib/api'

global.fetch = vi.fn()

describe('apiRequest', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should handle successful responses', async () => {
    const mockData = { id: 1, name: 'Test' }
    ;(fetch as any).mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(mockData),
    })

    const result = await apiRequest('/test')
    expect(result).toEqual(mockData)
    expect(fetch).toHaveBeenCalledWith(expect.stringContaining('/test'), expect.any(Object))
  })

  it('should throw error on failure', async () => {
    ;(fetch as any).mockResolvedValue({
      ok: false,
      status: 500,
      json: () => Promise.resolve({ message: 'Server Error' }),
    })

    await expect(apiRequest('/fail')).rejects.toThrow('Server Error')
  })
})
