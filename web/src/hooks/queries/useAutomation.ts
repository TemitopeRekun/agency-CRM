import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';

export interface TaskTemplate {
  id: string;
  serviceType: string;
  taskTitle: string;
  taskDescription: string;
  defaultPriority: string;
  tenantId: string;
}

export function useAutomation() {
  const queryClient = useQueryClient();

  const templatesQuery = useQuery({
    queryKey: ['automation-templates'],
    queryFn: async () => {
      const response = await api.get<TaskTemplate[]>('/automation/templates');
      return response.data;
    },
  });

  const createTemplateMutation = useMutation({
    mutationFn: async (template: Partial<TaskTemplate>) => {
      const response = await api.post<TaskTemplate>('/automation/templates', template);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['automation-templates'] });
    },
  });

  const deleteTemplateMutation = useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/automation/templates/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['automation-templates'] });
    },
  });

  const triggerOverdueCheckMutation = useMutation({
    mutationFn: async () => {
      const response = await api.post('/automation/trigger-overdue-check');
      return response.data;
    },
  });

  return {
    templates: templatesQuery.data ?? [],
    isLoading: templatesQuery.isLoading,
    createTemplate: createTemplateMutation.mutateAsync,
    deleteTemplate: deleteTemplateMutation.mutateAsync,
    triggerOverdueCheck: triggerOverdueCheckMutation.mutateAsync,
    isTriggering: triggerOverdueCheckMutation.isPending,
  };
}
