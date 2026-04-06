import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useOffers } from './useOffers'
import { useContracts } from './useContracts'
import { useContractPortal } from './useContractPortal'
import { vi, describe, it, expect, beforeAll, afterEach, afterAll } from 'vitest'
import { setupServer } from 'msw/node'
import { http, HttpResponse } from 'msw'

const API_URL = 'http://localhost:8000/api'

const handlers = [
  http.get(`${API_URL}/offers`, () => HttpResponse.json([{ id: 'O1', title: 'Offer 1' }])),
  http.get(`${API_URL}/contracts`, () => HttpResponse.json([{ id: 'C1', status: 1 }])), // 1 = Signed
  http.get(`${API_URL}/portal/contracts/:token`, () => HttpResponse.json({ id: 'C1', portalToken: 'test-token' })),
  http.post(`${API_URL}/portal/contracts/:token/sign`, () => 
    HttpResponse.json({ id: 'C1', portalToken: 'test-token', status: 1 }))
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

describe('Sales & Legal Hooks', () => {
    it('useOffers fetches successfully', async () => {
        const { result } = renderHook(() => useOffers(), { wrapper: createWrapper() })
        await waitFor(() => expect(result.current.isLoading).toBe(false))
        expect(result.current.offers).toHaveLength(1)
    })

    it('useContracts fetches successfully', async () => {
        const { result } = renderHook(() => useContracts(), { wrapper: createWrapper() })
        await waitFor(() => expect(result.current.isLoading).toBe(false))
        expect(result.current.contracts).toHaveLength(1)
    })

    it('useContractPortal signs successfully', async () => {
        const token = 'test-token'
        const { result } = renderHook(() => useContractPortal(token), { wrapper: createWrapper() })
        
        await result.current.sign('DigitSig')
        expect(result.current.contract?.portalToken).toBe(token)
    })
})
