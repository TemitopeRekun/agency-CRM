import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useLeads, LeadStatus } from './useLeads'
import { useInvoices, InvoiceStatus } from './useInvoices'
import { vi, describe, it, expect, beforeAll, afterEach, afterAll } from 'vitest'
import { setupServer } from 'msw/node'
import { http, HttpResponse } from 'msw'

const API_URL = 'http://localhost:8000/api'

const handlers = [
  http.get(`${API_URL}/leads`, () => HttpResponse.json([{ id: 'L1', title: 'Lead 1' }])),
  http.patch(`${API_URL}/leads/:id/status`, async ({ request }) => {
    const body = await request.json() as any
    return HttpResponse.json({ id: 'L1', status: body.status })
  }),
  http.get(`${API_URL}/invoices`, () => HttpResponse.json([{ id: 'I1', invoiceNumber: 'INV-1' }])),
  http.post(`${API_URL}/invoices/:id/payments`, async ({ request }) => {
    const body = await request.json() as any
    return HttpResponse.json({ id: 'I1', paidAmount: body.amount, status: 3 }) // 3 = Paid
  })
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

describe('Business Hooks', () => {
    describe('useLeads', () => {
        it('fetches leads correctly', async () => {
            const { result } = renderHook(() => useLeads(), { wrapper: createWrapper() })
            await waitFor(() => expect(result.current.isLoading).toBe(false))
            expect(result.current.leads).toHaveLength(1)
        })

        it('updates lead status correctly', async () => {
            const { result } = renderHook(() => useLeads(), { wrapper: createWrapper() })
            const updated = await result.current.updateStatus({ id: 'L1', status: LeadStatus.Qualified })
            expect(updated.status).toBe(LeadStatus.Qualified)
        })
    })

    describe('useInvoices', () => {
        it('fetches invoices correctly', async () => {
            const { result } = renderHook(() => useInvoices(), { wrapper: createWrapper() })
            await waitFor(() => expect(result.current.isLoading).toBe(false))
            expect(result.current.invoices).toHaveLength(1)
        })

        it('records payment and updates status', async () => {
            const { result } = renderHook(() => useInvoices(), { wrapper: createWrapper() })
            const updated = await result.current.recordPayment({ 
                id: 'I1', 
                data: { amount: 100, paymentDate: new Date().toISOString(), method: 1 } 
            })
            expect(updated.status).toBe(3) // Paid
        })
    })
})
