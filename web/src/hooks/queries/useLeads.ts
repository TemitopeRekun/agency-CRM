import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';

export enum LeadStatus {
  New = 0,
  Contacted = 1,
  Qualified = 2,
  Lost = 3
}

export enum LeadSource {
    Facebook = 0,
    Google = 1,
    Website = 2,
    Referral = 3,
    Manual = 4
}

export enum ServiceType {
    Development = 0,
    Marketing = 1,
    Staffing = 2,
    Other = 3
}

export enum PipelineStage {
    Discovery = 0,
    Proposal = 1,
    Negotiation = 2
}

export interface Lead {
  id: string;
  title: string;
  description: string;
  contactName: string;
  companyName: string;
  email: string;
  phone: string;
  source: LeadSource;
  interest: ServiceType;
  budgetRange: string;
  status: LeadStatus;
  pipelineStage: PipelineStage;
  probability: number;
  dealValue?: number;
  ownerId?: string;
  createdAt: string;
}

export const useLeads = () => {
  const queryClient = useQueryClient();

  const leadsQuery = useQuery({
    queryKey: ['leads'],
    queryFn: () => api.get<Lead[]>('/api/leads'),
  });

  const createLeadMutation = useMutation({
    mutationFn: (newLead: Partial<Lead>) =>
      api.post<Lead>('/api/leads', newLead),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leads'] });
      queryClient.invalidateQueries({ queryKey: ['stats'] });
    },
  });

  const updateLeadMutation = useMutation({
    mutationFn: ({ id, lead }: { id: string; lead: Partial<Lead> }) =>
      api.put<Lead>(`/api/leads/${id}`, lead),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leads'] });
    },
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: LeadStatus }) =>
      api.patch<Lead>(`/api/leads/${id}/status`, { status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leads'] });
    },
  });

  return {
    leads: leadsQuery.data ?? [],
    isLoading: leadsQuery.isLoading,
    error: leadsQuery.error,
    createLead: createLeadMutation.mutateAsync,
    isCreating: createLeadMutation.isPending,
    updateLead: updateLeadMutation.mutateAsync,
    isUpdating: updateLeadMutation.isPending,
    updateStatus: updateStatusMutation.mutateAsync,
    isUpdatingStatus: updateStatusMutation.isPending,
  };
};
