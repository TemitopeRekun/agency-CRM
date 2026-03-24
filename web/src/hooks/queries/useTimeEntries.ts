import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';

export interface TimeEntry {
  id: string;
  projectId: string;
  userId: string;
  hours: number;
  description: string;
  date: string;
  createdAt: string;
}

export const useTimeEntries = (projectId?: string) => {
  const queryClient = useQueryClient();

  const entriesQuery = useQuery({
    queryKey: ['timeEntries', projectId],
    queryFn: () => projectId 
      ? api.get<TimeEntry[]>(`/api/timeentries/project/${projectId}`)
      : api.get<TimeEntry[]>('/api/timeentries'),
  });

  const createEntryMutation = useMutation({
    mutationFn: (newEntry: Partial<TimeEntry>) =>
      api.post<TimeEntry>('/api/timeentries', newEntry),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['timeEntries', projectId] });
    },
  });

  return {
    entries: entriesQuery.data ?? [],
    isLoading: entriesQuery.isLoading,
    createEntry: createEntryMutation.mutateAsync,
    isCreating: createEntryMutation.isPending,
  };
};
