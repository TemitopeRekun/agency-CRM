import { render, screen, fireEvent } from '@testing-library/react'
import { Input } from './Input'
import { Select } from './Select'
import { Modal } from './Modal'
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from './Table'
import { describe, it, expect, vi } from 'vitest'

describe('UI Primitives', () => {
    describe('Input', () => {
        it('renders with label and handles change', () => {
            const onChange = vi.fn()
            render(<Input label="Test Label" value="" onChange={onChange} />)
            
            const input = screen.getByLabelText('Test Label')
            fireEvent.change(input, { target: { value: 'new value' } })
            expect(onChange).toHaveBeenCalled()
        })
    })

    describe('Select', () => {
        it('renders options correctly', () => {
            render(
                <Select label="My Select" options={[
                    { value: '1', label: 'Opt 1' },
                    { value: '2', label: 'Opt 2' }
                ]} />
            )
            expect(screen.getByText('Opt 1')).toBeInTheDocument()
        })
    })

    describe('Modal', () => {
        it('does not render when closed', () => {
            render(<Modal isOpen={false} title="Modal Title" onClose={() => {}}>Content</Modal>)
            expect(screen.queryByText('Modal Title')).not.toBeInTheDocument()
        })

        it('renders when open', () => {
            render(<Modal isOpen={true} title="Modal Title" onClose={() => {}}>Content</Modal>)
            expect(screen.getByText('Modal Title')).toBeInTheDocument()
        })
    })

    describe('Table', () => {
        it('renders table structure correctly', () => {
            render(
                <Table>
                    <TableHeader><TableRow><TableHead>Head 1</TableHead></TableRow></TableHeader>
                    <TableBody><TableRow><TableCell>Cell 1</TableCell></TableRow></TableBody>
                </Table>
            )
            expect(screen.getByText('Head 1')).toBeInTheDocument()
            expect(screen.getByText('Cell 1')).toBeInTheDocument()
        })
    })
})
