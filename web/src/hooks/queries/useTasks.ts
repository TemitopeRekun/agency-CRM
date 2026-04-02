import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';

export interface Task {
  id: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  projectId?: string;
  startDate: string;
  dueDate?: string;
  createdAt: string;
}

export const useTasks = () => {
  const queryClient = useQueryClient();

  const tasksQuery = useQuery({
    queryKey: ['tasks'],
    queryFn: () => api.get<Task[]>('/api/tasks'),
  });

  const createTaskMutation = useMutation({
    mutationFn: (newTask: Partial<Task>) =>
      api.post<Task>('/api/tasks', newTask),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
    },
  });

  return {
    tasks: tasksQuery.data ?? [],
    isLoading: tasksQuery.isLoading,
    error: tasksQuery.error,
    createTask: createTaskMutation.mutateAsync,
    isCreating: createTaskMutation.isPending,
  };
};
