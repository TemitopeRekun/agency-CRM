import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useProjects } from './useProjects'
import { useTasks } from './useTasks'
import { useStats } from './useStats'
import { useTimeEntries } from './useTimeEntries'
import { vi, describe, it, expect, beforeAll, afterEach, afterAll } from 'vitest'
import { setupServer } from 'msw/node'
import { http, HttpResponse } from 'msw'

const API_URL = 'http://localhost:8000/api'

const handlers = [
  http.get(`${API_URL}/projects`, () => HttpResponse.json([{ id: 'P1', name: 'Project 1' }])),
  http.get(`${API_URL}/tasks`, () => HttpResponse.json([{ id: 'T1', title: 'Task 1' }])),
  http.get(`${API_URL}/stats`, () => HttpResponse.json({ clientsCount: 10, revenueTotal: 50000 })),
  http.get(`${API_URL}/timeentries`, () => HttpResponse.json([{ id: 'TE1', hours: 5 }]))
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

describe('Project Monitoring Hooks', () => {
    it('useProjects fetches successfully', async () => {
        const { result } = renderHook(() => useProjects(), { wrapper: createWrapper() })
        await waitFor(() => expect(result.current.isLoading).toBe(false))
        expect(result.current.projects).toHaveLength(1)
    })

    it('useTasks fetches successfully', async () => {
        const { result } = renderHook(() => useTasks(), { wrapper: createWrapper() })
        await waitFor(() => expect(result.current.isLoading).toBe(false))
        expect(result.current.tasks).toHaveLength(1)
    })

    it('useStats fetches successfully', async () => {
        const { result } = renderHook(() => useStats(), { wrapper: createWrapper() })
        await waitFor(() => expect(result.current.isLoading).toBe(false))
        expect(result.current.data?.clients).toBe(10)
    })

    it('useTimeEntries fetches successfully', async () => {
        const { result } = renderHook(() => useTimeEntries('P1'), { wrapper: createWrapper() })
        await waitFor(() => expect(result.current.isLoading).toBe(false))
        expect(result.current.entries).toHaveLength(1)
    })
})
