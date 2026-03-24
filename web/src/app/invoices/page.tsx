'use client';

import { useInvoices, InvoiceStatus } from '@/hooks/queries/useInvoices';
import { useProjects } from '@/hooks/queries/useProjects';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Container, Section } from '@/components/ui/LayoutPrimitives';

import { Button } from '@/components/ui/Button';
import { toast } from 'sonner';
import { useState } from 'react';
import { InvoiceEditModal } from './components/InvoiceEditModal';
import { Invoice } from '@/hooks/queries/useInvoices';

export default function InvoicesPage() {
  const { invoices, isLoading, updateStatus, isUpdatingStatus } = useInvoices();
  const { projects } = useProjects();
  
  const [editingInvoice, setEditingInvoice] = useState<Invoice | null>(null);

  const handleMarkAsPaid = async (id: string) => {
    try {
      await updateStatus({ id, status: InvoiceStatus.Paid });
      toast.success('Invoice marked as Paid');
    } catch (err) {
      toast.error('Failed to update invoice status');
    }
  };

  const getProjectName = (projectId: string) => {
    const project = projects.find(p => p.id === projectId);
    return project ? project.name : 'Unknown Project';
  };

  const getStatusColor = (status: InvoiceStatus) => {
    switch (status) {
      case InvoiceStatus.Paid: return 'bg-emerald-100 text-emerald-800';
      case InvoiceStatus.Overdue: return 'bg-rose-100 text-rose-800';
      case InvoiceStatus.Draft: return 'bg-slate-100 text-slate-800';
      default: return 'bg-blue-100 text-blue-800';
    }
  };

  return (
    <Container>
      <Section className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Invoices</h1>
      </Section>

      <Section>
        {isLoading ? (
          <div className="space-y-4">
            {[1, 2, 3].map((i) => (
              <div key={i} className="h-12 w-full bg-muted animate-pulse rounded" />
            ))}
          </div>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Invoice #</TableHead>
                <TableHead>Project</TableHead>
                <TableHead>Amount</TableHead>
                <TableHead>Due Date</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Created At</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {invoices.map((inv) => (
                <TableRow key={inv.id}>
                  <TableCell className="font-mono text-xs">{inv.invoiceNumber}</TableCell>
                  <TableCell>{getProjectName(inv.projectId)}</TableCell>
                  <TableCell className="font-bold">
                    {inv.currency} {inv.totalAmount?.toLocaleString()}
                  </TableCell>
                  <TableCell>{new Date(inv.dueDate).toLocaleDateString()}</TableCell>
                  <TableCell>
                    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${getStatusColor(inv.status)}`}>
                      {InvoiceStatus[inv.status]}
                    </span>
                  </TableCell>
                  <TableCell>{new Date(inv.createdAt).toLocaleDateString()}</TableCell>
                  <TableCell className="text-right">
                    <div className="flex gap-2 justify-end">
                      <Button 
                        size="sm" 
                        variant="ghost" 
                        onClick={() => setEditingInvoice(inv)}
                      >
                        Edit
                      </Button>
                      {inv.status !== InvoiceStatus.Paid && (
                        <Button 
                          size="sm" 
                          variant="outline" 
                          onClick={() => handleMarkAsPaid(inv.id)}
                          disabled={isUpdatingStatus}
                          className="text-emerald-600 border-emerald-200 hover:bg-emerald-50"
                        >
                          Mark Paid
                        </Button>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              ))}
              {invoices.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6} className="text-center text-muted-foreground py-8">
                    No invoices found. Generate one from a Project or Contract.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        )}
      </Section>
      
      <InvoiceEditModal
        isOpen={!!editingInvoice}
        onClose={() => setEditingInvoice(null)}
        invoice={editingInvoice}
      />
    </Container>
  );
}
