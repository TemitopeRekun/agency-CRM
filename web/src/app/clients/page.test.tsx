import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import ClientsPage from './page'
import { vi, describe, it, expect } from 'vitest'
import { useClients, PriorityTier } from '@/hooks/queries/useClients'

vi.mock('@/hooks/queries/useClients', () => ({
  useClients: vi.fn(),
  PriorityTier: {
    Tier1: 0,
    Tier2: 1,
    Tier3: 2,
    0: 'Tier1 (High)',
    1: 'Tier2 (Medium)',
    2: 'Tier3 (Low)',
  },
}))

describe('ClientsPage Component', () => {
  const mockClients = [
    { id: '1', name: 'Zebra Corp', legalName: 'Zebra Solutions', industry: 'Tech', priority: 0, createdAt: '2024-01-01' },
    { id: '2', name: 'Alpha Inc', legalName: 'Alpha Ltd', industry: 'Finance', priority: 1, createdAt: '2024-01-02' },
  ]

  it('renders loading state correctly', () => {
    (useClients as any).mockReturnValue({ clients: [], isLoading: true })
    render(<ClientsPage />)

    // Animated pulses have bg-muted class
    const pulses = document.getElementsByClassName('animate-pulse');
    expect(pulses.length).toBeGreaterThan(0);
  })

  it('renders clients table correctly', () => {
    (useClients as any).mockReturnValue({ clients: mockClients, isLoading: false })
    render(<ClientsPage />)

    expect(screen.getByText('Zebra Corp')).toBeInTheDocument()
    expect(screen.getByText('Alpha Inc')).toBeInTheDocument()
  })

  it('opens modal on "Add Client" click', () => {
    (useClients as any).mockReturnValue({ clients: mockClients, isLoading: false })
    render(<ClientsPage />)

    fireEvent.click(screen.getByRole('button', { name: /Add Client/i }))
    expect(screen.getByText('Create New Client')).toBeInTheDocument()
  })

  it('sorts clients correctly by name', async () => {
    (useClients as any).mockReturnValue({ clients: mockClients, isLoading: false })
    render(<ClientsPage />)

    fireEvent.change(screen.getByRole('combobox'), { target: { value: 'name' } })

    const rows = screen.getAllByRole('row')
    // Header is row 0, Alpha should be row 1, Zebra row 2
    expect(rows[1]).toHaveTextContent('Alpha Inc')
    expect(rows[2]).toHaveTextContent('Zebra Corp')
  })
})
