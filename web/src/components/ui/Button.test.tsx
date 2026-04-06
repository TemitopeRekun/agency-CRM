import { render, screen, fireEvent } from '@testing-library/react'
import { Button } from './Button'
import { describe, it, expect, vi } from 'vitest'

describe('Button Component', () => {
    it('renders with children correctly', () => {
        render(<Button>Click Me</Button>)
        expect(screen.getByText('Click Me')).toBeInTheDocument()
    })

    it('handles onClick events', () => {
        const handleClick = vi.fn()
        render(<Button onClick={handleClick}>Click Me</Button>)
        
        fireEvent.click(screen.getByText('Click Me'))
        expect(handleClick).toHaveBeenCalledTimes(1)
    })

    it('shows loading spinner and disables when isLoading is true', () => {
        render(<Button isLoading>Click Me</Button>)
        
        const button = screen.getByRole('button')
        expect(button).toBeDisabled()
        expect(button.querySelector('.animate-spin')).toBeInTheDocument()
    })

    it('applies correct variant styles', () => {
        const { rerender } = render(<Button variant="destructive">Delete</Button>)
        expect(screen.getByRole('button')).toHaveClass('bg-destructive')

        rerender(<Button variant="outline">Back</Button>)
        expect(screen.getByRole('button')).toHaveClass('border-input')
    })
})
