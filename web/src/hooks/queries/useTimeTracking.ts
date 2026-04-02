import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';

export interface TimeEntry {
  id: string;
  projectId: string;
  projectName: string;
  taskId: string;
  taskTitle: string;
  userId: string;
  userName: string;
  hours: number;
  description: string;
  date: string;
}

export interface ProjectMember {
  userId: string;
  userName: string;
  email: string;
  role: string;
  hourlyRate: number;
}

export interface ProjectTeamResponse {
  projectId: string;
  members: ProjectMember[];
  totalHours: number;
  estimatedLaborCost: number;
}

export const useTimeTracking = (projectId?: string) => {
  const queryClient = useQueryClient();

  const timeEntriesQuery = useQuery({
    queryKey: ['timeTracking', 'entries', projectId],
    queryFn: () => projectId 
      ? api.get<TimeEntry[]>(`/api/timetracking/project/${projectId}`)
      : api.get<TimeEntry[]>('/api/timetracking'),
    enabled: !!projectId,
  });

  const projectTeamQuery = useQuery({
    queryKey: ['timeTracking', 'team', projectId],
    queryFn: () => api.get<ProjectTeamResponse>(`/api/timetracking/project/${projectId}/team`),
    enabled: !!projectId,
  });

  const logTimeMutation = useMutation({
    mutationFn: (data: { projectId: string; taskId: string; hours: number; description: string; date?: string }) =>
      api.post<TimeEntry>('/api/timetracking', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['timeTracking'] });
      queryClient.invalidateQueries({ queryKey: ['tasks'] }); // Refresh tasks to show new time
    },
  });

  const addTeamMemberMutation = useMutation({
    mutationFn: (data: { userId: string; role: string }) =>
      api.post(`/api/timetracking/project/${projectId}/team`, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['timeTracking', 'team', projectId] });
    },
  });

  return {
    timeEntries: timeEntriesQuery.data ?? [],
    isTimeEntriesLoading: timeEntriesQuery.isLoading,
    projectTeam: projectTeamQuery.data,
    isProjectTeamLoading: projectTeamQuery.isLoading,
    logTime: logTimeMutation.mutateAsync,
    isLoggingTime: logTimeMutation.isPending,
    addTeamMember: addTeamMemberMutation.mutateAsync,
    isAddingTeamMember: addTeamMemberMutation.isPending,
  };
};
