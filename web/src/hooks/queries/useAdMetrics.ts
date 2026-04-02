import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';

export enum AdPlatform {
  Google,
  Meta,
  TikTok,
  LinkedIn
}

export interface AdMetric {
  id: string;
  projectId: string;
  platform: AdPlatform;
  spend: number;
  impressions: number;
  clicks: number;
  conversions: number;
  date: string;
  createdAt: string;
}

export interface AdMetricAnalytics {
  projectId: string;
  totalSpend: number;
  totalImpressions: number;
  totalClicks: number;
  totalConversions: number;
  costPerLead: number;
  conversionRate: number;
  ctr: number;
  roas: number;
  projectROI: number;
  rawMetrics: AdMetric[];
}

export const useAdMetrics = (projectId?: string) => {
  const queryClient = useQueryClient();

  const metricsQuery = useQuery({
    queryKey: ['adMetrics', projectId],
    queryFn: () => projectId 
      ? api.get<AdMetric[]>(`/api/admetrics/project/${projectId}`) 
      : api.get<AdMetric[]>('/api/admetrics'),
  });

  const analyticsQuery = useQuery({
    queryKey: ['adMetrics', 'analytics', projectId],
    queryFn: () => projectId 
      ? api.get<AdMetricAnalytics>(`/api/admetrics/project/${projectId}/analytics`)
      : api.get<AdMetricAnalytics>('/api/admetrics/analytics'),
  });

  const createMetricMutation = useMutation({
    mutationFn: (newMetric: Partial<AdMetric>) =>
      api.post<AdMetric>('/api/admetrics', newMetric),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adMetrics', projectId] });
    },
  });

  return {
    metrics: metricsQuery.data ?? [],
    isLoading: metricsQuery.isLoading,
    analytics: analyticsQuery.data,
    isAnalyticsLoading: analyticsQuery.isLoading,
    createMetric: createMetricMutation.mutateAsync,
    isCreating: createMetricMutation.isPending,
  };
};
