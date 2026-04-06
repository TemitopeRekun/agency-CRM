import { render, act, screen, renderHook } from '@testing-library/react'
import { AuthProvider, useAuth } from './useAuth'
import { vi, describe, it, expect, beforeAll, afterEach, afterAll } from 'vitest'
import { setupServer } from 'msw/node'
import { handlers } from '../test/handlers'

const server = setupServer(...handlers)

const mockPush = vi.fn()
vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}))

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  vi.clearAllMocks()
})
afterAll(() => server.close())

describe('useAuth Hook', () => {
  it('logs in successfully and updates user state', async () => {
    const { result } = renderHook(() => useAuth(), {
      wrapper: AuthProvider,
    })

    await act(async () => {
      await result.current.login('admin@example.com', 'Admin123!')
    })

    expect(result.current.user).toEqual({
      id: '1',
      email: 'admin@example.com',
      fullName: 'Admin User',
    })
    expect(mockPush).toHaveBeenCalledWith('/dashboard')
  })

  it('logs out successfully and clears user state', async () => {
    const { result } = renderHook(() => useAuth(), {
      wrapper: AuthProvider,
    })

    // Pre-set user state if possible (or login first)
    await act(async () => {
      await result.current.login('admin@example.com', 'Admin123!')
    })

    await act(async () => {
      await result.current.logout()
    })

    expect(result.current.user).toBeNull()
    expect(mockPush).toHaveBeenCalledWith('/login')
  })
})
