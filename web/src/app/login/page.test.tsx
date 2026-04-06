import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import LoginPage from './page'
import { vi, describe, it, expect } from 'vitest'
import { useAuth } from '@/hooks/useAuth'

vi.mock('@/hooks/useAuth', () => ({
  useAuth: vi.fn(),
}))

describe('LoginPage Component', () => {
  it('renders login form correctly', () => {
    (useAuth as any).mockReturnValue({ login: vi.fn() })
    render(<LoginPage />)

    expect(screen.getByLabelText(/Email Address/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/Password/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Sign In/i })).toBeInTheDocument()
  })

  it('calls login function on form submission', async () => {
    const mockLogin = vi.fn().mockResolvedValue({})
    (useAuth as any).mockReturnValue({ login: mockLogin })

    render(<LoginPage />)

    fireEvent.change(screen.getByLabelText(/Email Address/i), {
      target: { value: 'test@example.com' },
    })
    fireEvent.change(screen.getByLabelText(/Password/i), {
      target: { value: 'password123' },
    })

    fireEvent.click(screen.getByRole('button', { name: /Sign In/i }))

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith('test@example.com', 'password123')
    })
  })

  it('displays error message on failed login', async () => {
    const mockLogin = vi.fn().mockRejectedValue(new Error('Invalid credentials'))
    (useAuth as any).mockReturnValue({ login: mockLogin })

    render(<LoginPage />)

    fireEvent.click(screen.getByRole('button', { name: /Sign In/i }))

    await waitFor(() => {
      expect(screen.getByText(/Invalid credentials/i)).toBeInTheDocument()
    })
  })
})
