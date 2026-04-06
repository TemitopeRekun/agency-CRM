import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useClients } from './useClients'
import { vi, describe, it, expect, beforeAll, afterEach, afterAll } from 'vitest'
import { setupServer } from 'msw/node'
import { handlers } from '../../test/handlers'

const server = setupServer(...handlers)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  vi.clearAllMocks()
})
afterAll(() => server.close())

const createWrapper = () => {
    const queryClient = new QueryClient({
        defaultOptions: {
            queries: {
                retry: false,
            },
        },
    })
    return ({ children }: { children: React.ReactNode }) => (
        <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    )
}

describe('useClients Hook', () => {
    it('fetches clients successfully', async () => {
        const { result } = renderHook(() => useClients(), {
            wrapper: createWrapper(),
        })

        await waitFor(() => expect(result.current.isLoading).toBe(false))

        expect(result.current.clients).toHaveLength(2)
        expect(result.current.clients[0].name).toBe('Mock Client 1')
    })

    it('creates a new client and handles mutation', async () => {
        const { result } = renderHook(() => useClients(), {
            wrapper: createWrapper(),
        })

        const newClient = { name: 'New Client' }
        const createdClient = await result.current.createClient(newClient)

        expect(createdClient.name).toBe('New Client')
        expect(createdClient.id).toBe('3')
    })
})
