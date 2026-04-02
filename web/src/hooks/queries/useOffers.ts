import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';

export enum OfferStatus {
  Draft = 0,
  Sent = 1,
  Accepted = 2,
  Rejected = 3
}

export interface Offer {
  id: string;
  title: string;
  totalAmount: number;
  status: OfferStatus;
  notes: string;
  leadId: string;
  quoteTemplateId?: string;
  quoteOpenedAt?: string;
  hasBeenViewed?: boolean;
  createdAt: string;
}

export const useOffers = () => {
  const queryClient = useQueryClient();

  const offersQuery = useQuery({
    queryKey: ['offers'],
    queryFn: () => api.get<Offer[]>('/api/offers'),
  });

  const createOfferMutation = useMutation({
    mutationFn: (newOffer: Partial<Offer>) =>
      api.post<Offer>('/api/offers', newOffer),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['offers'] });
    },
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: OfferStatus }) =>
      api.patch<Offer>(`/api/offers/${id}/status`, { status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['offers'] });
    },
  });

  const markAsViewedMutation = useMutation({
    mutationFn: (id: string) => api.post<Offer>(`/api/offers/${id}/view`, {}),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['offers'] });
    },
  });

  return {
    offers: offersQuery.data ?? [],
    isLoading: offersQuery.isLoading,
    error: offersQuery.error,
    createOffer: createOfferMutation.mutateAsync,
    isCreating: createOfferMutation.isPending,
    updateStatus: updateStatusMutation.mutateAsync,
    isUpdatingStatus: updateStatusMutation.isPending,
    markAsViewed: markAsViewedMutation.mutateAsync,
    isMarkingAsViewed: markAsViewedMutation.isPending,
  };
};
