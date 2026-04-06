import { render, screen } from '@testing-library/react'
import { GanttChart } from './GanttChart'
import { describe, it, expect } from 'vitest'
import { addDays, startOfToday } from 'date-fns'

describe('GanttChart Component', () => {
    const today = startOfToday()
    const mockTasks = [
        {
            id: '1',
            title: 'Task 1',
            startDate: today,
            endDate: addDays(today, 5),
            status: 'completed',
            progress: 100
        },
        {
            id: '2',
            title: 'Task 2',
            startDate: addDays(today, 2),
            endDate: addDays(today, 10),
            status: 'InProgress',
            progress: 50
        }
    ]

    it('renders empty matching state when no tasks provided', () => {
        render(<GanttChart tasks={[]} />)
        expect(screen.getByText(/No tasks scheduled/i)).toBeInTheDocument()
    })

    it('renders tasks correctly on the timeline', () => {
        render(<GanttChart tasks={mockTasks} />)
        expect(screen.getByText('Task 1')).toBeInTheDocument()
        expect(screen.getByText('Task 2')).toBeInTheDocument()
        expect(screen.getByText('100% Complete')).toBeInTheDocument()
        expect(screen.getByText('50% Complete')).toBeInTheDocument()
    })

    it('calculates date headers correctly', () => {
        render(<GanttChart tasks={mockTasks} />)
        // Check for day numbers or month names
        const dayNumber = today.getDate().toString()
        const days = screen.getAllByText(dayNumber)
        expect(days.length).toBeGreaterThan(0)
    })
})
