import { render, screen } from '@testing-library/react'
import { Breadcrumbs } from './Breadcrumbs'
import { Card, CardHeader, CardTitle, CardContent, Container, Section } from './LayoutPrimitives'
import { vi, describe, it, expect } from 'vitest'

// Mock next/navigation
vi.mock('next/navigation', () => ({
    usePathname: () => '/clients'
}))

describe('Advanced UI Primitives', () => {
    describe('Breadcrumbs', () => {
        it('renders correct hierarchy based on pathname', () => {
            render(<Breadcrumbs />)
            expect(screen.getByText('Home')).toBeInTheDocument()
            expect(screen.getByText('Clients')).toBeInTheDocument()
        })
    })

    describe('LayoutPrimitives', () => {
        it('renders Card components correctly', () => {
            render(
                <Card>
                    <CardHeader><CardTitle>Header</CardTitle></CardHeader>
                    <CardContent>Content</CardContent>
                </Card>
            )
            expect(screen.getByText('Header')).toBeInTheDocument()
            expect(screen.getByText('Content')).toBeInTheDocument()
        })

        it('renders Container and Section with correct structure', () => {
            render(
                <Container>
                    <Section title="My Section">Section Content</Section>
                </Container>
            )
            expect(screen.getByText('My Section')).toBeInTheDocument()
            expect(screen.getByText('Section Content')).toBeInTheDocument()
        })
    })
})
