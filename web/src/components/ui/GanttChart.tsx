'use client';

import React from 'react';
import { format, differenceInDays, startOfMonth, addMonths, endOfMonth, eachDayOfInterval, isSameDay } from 'date-fns';
import { cn } from '@/lib/utils';

interface GanttTask {
  id: string;
  title: string;
  startDate: Date;
  endDate: Date;
  status: string;
  progress: number;
}

interface GanttChartProps {
  tasks: GanttTask[];
}

export function GanttChart({ tasks }: GanttChartProps) {
  if (!tasks || tasks.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center p-12 bg-gray-50 border border-dashed border-gray-200 rounded-xl text-center">
        <p className="text-gray-500 font-medium">No tasks scheduled for this project timeline.</p>
      </div>
    );
  }

  // Calculate timeline bounds
  const earliestDate = new Date(Math.min(...tasks.map(t => t.startDate.getTime())));
  const latestDate = new Date(Math.max(...tasks.map(t => t.endDate.getTime())));
  
  const chartStart = startOfMonth(earliestDate);
  const chartEnd = endOfMonth(addMonths(latestDate, 1));
  
  const totalDays = differenceInDays(chartEnd, chartStart) + 1;
  const days = eachDayOfInterval({ start: chartStart, end: chartEnd });

  // Get month boundaries for headers
  const months: { name: string; startDay: number; length: number }[] = [];
  let currentMonth = chartStart;
  while (currentMonth <= chartEnd) {
    const monthEnd = endOfMonth(currentMonth);
    const actualEnd = monthEnd > chartEnd ? chartEnd : monthEnd;
    const length = differenceInDays(actualEnd, currentMonth) + 1;
    
    months.push({
      name: format(currentMonth, 'MMMM yyyy'),
      startDay: differenceInDays(currentMonth, chartStart),
      length
    });
    
    currentMonth = addMonths(currentMonth, 1);
    currentMonth = startOfMonth(currentMonth);
  }

  return (
    <div className="bg-white border border-gray-200 rounded-xl overflow-hidden shadow-sm">
      <div className="overflow-x-auto custom-scrollbar">
        <div style={{ minWidth: `${totalDays * 32 + 200}px` }} className="relative">
          {/* Timeline Header */}
          <div className="flex border-b border-gray-100 bg-gray-50/50">
            <div className="w-[200px] flex-shrink-0 border-r border-gray-200 p-4 font-bold text-xs text-gray-500 uppercase tracking-widest bg-white">
              Task Details
            </div>
            <div className="flex-1 bg-gray-50/50">
              {/* Months Row */}
              <div className="flex border-b border-gray-100">
                {months.map((month, i) => (
                  <div 
                    key={i} 
                    className="border-r border-gray-100 p-2 text-xs font-bold text-gray-700 text-center"
                    style={{ width: `${month.length * 32}px` }}
                  >
                    {month.name}
                  </div>
                ))}
              </div>
              {/* Days Row */}
              <div className="flex">
                {days.map((day, i) => (
                  <div 
                    key={i} 
                    className={cn(
                      "w-8 h-8 flex items-center justify-center text-[10px] border-r border-gray-50 text-gray-400 font-medium",
                      (day.getDay() === 0 || day.getDay() === 6) && "bg-gray-100/50"
                    )}
                  >
                    {format(day, 'd')}
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Grid Content */}
          <div className="relative">
            {tasks.map((task, taskIndex) => {
              const startOffset = differenceInDays(task.startDate, chartStart);
              const duration = differenceInDays(task.endDate, task.startDate) + 1;
              
              return (
                <div key={task.id} className="flex border-b border-gray-100 group hover:bg-gray-50/30 transition-colors">
                  <div className="w-[200px] flex-shrink-0 border-r border-gray-200 p-4 bg-white z-10">
                    <p className="text-sm font-semibold text-gray-900 truncate">{task.title}</p>
                    <p className="text-[10px] text-gray-500 mt-0.5 uppercase tracking-tighter">
                      {format(task.startDate, 'MMM d')} - {format(task.endDate, 'MMM d')}
                    </p>
                  </div>
                  
                  <div className="flex-1 relative h-16 bg-white/50">
                    {/* Background Grid Lines */}
                    <div className="absolute inset-0 flex pointer-events-none">
                      {days.map((day, i) => (
                        <div 
                          key={i} 
                          className={cn(
                            "w-8 h-full border-r border-gray-50",
                            (day.getDay() === 0 || day.getDay() === 6) && "bg-gray-100/20"
                          )} 
                        />
                      ))}
                    </div>

                    {/* Task Bar */}
                    <div 
                      className="absolute top-4 h-8 rounded-full shadow-sm flex items-center px-3 z-20 group-hover:scale-[1.02] transition-transform"
                      style={{ 
                        left: `${startOffset * 32 + 4}px`, 
                        width: `${duration * 32 - 8}px`,
                        backgroundColor: getStatusColor(task.status)
                      }}
                    >
                      {/* Progress Fill */}
                      <div 
                        className="absolute inset-0 bg-black/10 rounded-full" 
                        style={{ width: `${task.progress}%` }}
                      />
                      <span className="relative text-[10px] font-bold text-white truncate drop-shadow-sm">
                        {task.progress}% Complete
                      </span>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
}

function getStatusColor(status: string) {
  switch (status.toLowerCase()) {
    case 'completed': return '#10b981'; // Emerald 500
    case 'todo': return '#6366f1';      // Indigo 500
    case 'inprogress': return '#f59e0b'; // Amber 500
    default: return '#94a3b8';           // Slate 400
  }
}
