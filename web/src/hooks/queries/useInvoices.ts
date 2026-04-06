import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';

export enum InvoiceStatus {
  Draft,
  Sent,
  PartiallyPaid,
  Paid,
  Overdue,
  Cancelled
}

export interface InvoiceItem {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  amount: number;
}

export interface Payment {
  id: string;
  amount: number;
  paymentDate: string;
  method: number;
  referenceNumber: string;
  notes: string;
}

export interface Invoice {
  id: string;
  invoiceNumber: string;
  totalAmount: number;
  paidAmount: number;
  balanceAmount: number;
  currency: string;
  status: InvoiceStatus;
  dueDate: string;
  projectId: string;
  createdAt: string;
  items: InvoiceItem[];
  payments: Payment[];
}

export const useInvoices = () => {
  const queryClient = useQueryClient();

  const invoicesQuery = useQuery({
    queryKey: ['invoices'],
    queryFn: () => api.get<Invoice[]>('/api/invoices'),
  });

  const recordPaymentMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: { amount: number; paymentDate: string; method: number; referenceNumber?: string; notes?: string } }) =>
      api.post<Invoice>(`/api/invoices/${id}/payments`, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
    },
  });

  const generateFromContractMutation = useMutation({
    mutationFn: (contractId: string) =>
      api.post<Invoice>(`/api/invoices/generate/contract/${contractId}`, {}),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
    },
  });

  const generateFromProjectMutation = useMutation({
    mutationFn: (projectId: string) =>
      api.post<Invoice>(`/api/invoices/generate/project/${projectId}`, {}),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
    },
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: InvoiceStatus }) =>
      api.put<Invoice>(`/api/invoices/${id}/status`, { status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
    },
  });

  const updateInvoiceMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Invoice> }) =>
      api.put<Invoice>(`/api/invoices/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
    },
  });

  return {
    invoices: invoicesQuery.data ?? [],
    isLoading: invoicesQuery.isLoading,
    error: invoicesQuery.error,
    recordPayment: recordPaymentMutation.mutateAsync,
    isRecordingPayment: recordPaymentMutation.isPending,
    generateFromContract: generateFromContractMutation.mutateAsync,
    isGeneratingFromContract: generateFromContractMutation.isPending,
    generateFromProject: generateFromProjectMutation.mutateAsync,
    isGeneratingFromProject: generateFromProjectMutation.isPending,
    updateStatus: updateStatusMutation.mutateAsync,
    isUpdatingStatus: updateStatusMutation.isPending,
    updateInvoice: updateInvoiceMutation.mutateAsync,
    isUpdatingInvoice: updateInvoiceMutation.isPending,
  };
};
