import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAdAccounts } from './useAdAccounts'
import { useAdMetrics } from './useAdMetrics'
import { useAutomation } from './useAutomation'
import { vi, describe, it, expect, beforeAll, afterEach, afterAll } from 'vitest'
import { setupServer } from 'msw/node'
import { http, HttpResponse } from 'msw'

const API_URL = 'http://localhost:8000/api'

const handlers = [
  http.get(`${API_URL}/projectadaccounts`, () => HttpResponse.json([{ id: 'A1', platform: 0, name: 'FB Ads' }])),
  http.get(`${API_URL}/admetrics/analytics`, () => HttpResponse.json({ totalSpend: 1000, totalImpressions: 50000 })),
  http.post(`${API_URL}/automation/trigger`, () => new HttpResponse(null, { status: 204 }))
]

const server = setupServer(...handlers)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  vi.clearAllMocks()
})
afterAll(() => server.close())

const createWrapper = () => {
    const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
    })
    return ({ children }: { children: React.ReactNode }) => (
        <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    )
}

describe('Ads & Automation Hooks', () => {
    it('useAdAccounts fetches successfully', async () => {
        const { result } = renderHook(() => useAdAccounts('P1'), { wrapper: createWrapper() })
        await waitFor(() => expect(result.current.isLoading).toBe(false))
        expect(result.current.accounts).toHaveLength(1)
    })

    it('useAdMetrics fetches analytics successfully', async () => {
        const { result } = renderHook(() => useAdMetrics(), { wrapper: createWrapper() })
        await waitFor(() => expect(result.current.isAnalyticsLoading).toBe(false))
        expect(result.current.analytics?.totalSpend).toBe(1000)
    })

    it('useAutomation triggers successfully', async () => {
        const { result } = renderHook(() => useAutomation(), { wrapper: createWrapper() })
        await result.current.triggerOverdueCheck()
        // No error thrown is the success assertion
    })
})
