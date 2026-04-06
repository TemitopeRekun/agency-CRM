'use client';

import { useInvoices, InvoiceStatus } from '@/hooks/queries/useInvoices';
import { useProjects } from '@/hooks/queries/useProjects';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Container, Section } from '@/components/ui/LayoutPrimitives';
import { Modal } from '@/components/ui/Modal';
import { Input } from '@/components/ui/Input';

import { Button } from '@/components/ui/Button';
import { toast } from 'sonner';
import { useState } from 'react';
import { InvoiceEditModal } from './components/InvoiceEditModal';
import { Invoice } from '@/hooks/queries/useInvoices';

export default function InvoicesPage() {
  const { invoices, isLoading, updateStatus, isUpdatingStatus, recordPayment, isRecordingPayment } = useInvoices();
  const { projects } = useProjects();
  
  const [editingInvoice, setEditingInvoice] = useState<Invoice | null>(null);
  const [paymentModalInvoice, setPaymentModalInvoice] = useState<Invoice | null>(null);
  const [paymentData, setPaymentData] = useState({
      amount: 0,
      paymentDate: new Date().toISOString().split('T')[0],
      method: 0, // BankTransfer
      referenceNumber: '',
      notes: ''
  });

  const handleMarkAsPaid = async (id: string) => {
    try {
      await updateStatus({ id, status: InvoiceStatus.Paid });
      toast.success('Invoice marked as Paid');
    } catch {
      toast.error('Failed to update invoice status');
    }
  };

  const handleRecordPayment = async (e: React.FormEvent) => {
      e.preventDefault();
      if (!paymentModalInvoice) return;
      
      try {
          await recordPayment({ 
              id: paymentModalInvoice.id, 
              data: {
                  ...paymentData,
                  amount: Number(paymentData.amount)
              }
          });
          toast.success('Payment recorded successfully');
          setPaymentModalInvoice(null);
          setPaymentData({
              amount: 0,
              paymentDate: new Date().toISOString().split('T')[0],
              method: 0,
              referenceNumber: '',
              notes: ''
          });
      } catch {
          toast.error('Failed to record payment');
      }
  };

  const getProjectName = (projectId: string) => {
    const project = projects.find(p => p.id === projectId);
    return project ? project.name : 'Unknown Project';
  };

  const getStatusColor = (status: InvoiceStatus) => {
    switch (status) {
      case InvoiceStatus.Paid: return 'bg-emerald-100 text-emerald-800';
      case InvoiceStatus.PartiallyPaid: return 'bg-amber-100 text-amber-800';
      case InvoiceStatus.Overdue: return 'bg-rose-100 text-rose-800';
      case InvoiceStatus.Draft: return 'bg-slate-100 text-slate-800';
      default: return 'bg-blue-100 text-blue-800';
    }
  };

  return (
    <Container>
      <Section className="flex items-center justify-between">
        <div>
            <h1 className="text-3xl font-bold tracking-tight">Invoices</h1>
            <p className="text-muted-foreground mt-1">Track billings, payments, and account balances.</p>
        </div>
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
                <TableHead>Total</TableHead>
                <TableHead>Paid</TableHead>
                <TableHead>Balance</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Due Date</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {invoices.map((inv) => (
                <TableRow key={inv.id}>
                  <TableCell className="font-mono text-xs font-bold">{inv.invoiceNumber}</TableCell>
                  <TableCell>{getProjectName(inv.projectId)}</TableCell>
                  <TableCell className="font-medium text-slate-500">
                    {inv.currency} {inv.totalAmount?.toLocaleString()}
                  </TableCell>
                  <TableCell className="text-emerald-600 font-medium">
                    {inv.currency} {inv.paidAmount?.toLocaleString() || '0'}
                  </TableCell>
                  <TableCell className="font-bold">
                    {inv.currency} {inv.balanceAmount?.toLocaleString() || inv.totalAmount?.toLocaleString()}
                  </TableCell>
                  <TableCell>
                    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${getStatusColor(inv.status)}`}>
                      {InvoiceStatus[inv.status]}
                    </span>
                  </TableCell>
                  <TableCell className="text-xs">{new Date(inv.dueDate).toLocaleDateString()}</TableCell>
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
                          onClick={() => {
                              setPaymentModalInvoice(inv);
                              setPaymentData(prev => ({ ...prev, amount: inv.balanceAmount }));
                          }}
                          className="text-emerald-600 border-emerald-200 hover:bg-emerald-50"
                        >
                          Add Payment
                        </Button>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              ))}
              {invoices.length === 0 && (
                <TableRow>
                  <TableCell colSpan={8} className="text-center text-muted-foreground py-12">
                    <div className="font-medium text-lg text-slate-400">No invoices found.</div>
                    <p className="text-sm mt-1">Generate one from a Project or Contract to get started.</p>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        )}
      </Section>
      
      <InvoiceEditModal
        key={editingInvoice?.id}
        isOpen={!!editingInvoice}
        onClose={() => setEditingInvoice(null)}
        invoice={editingInvoice}
      />

      <Modal
        isOpen={!!paymentModalInvoice}
        onClose={() => setPaymentModalInvoice(null)}
        title={`Add Payment for ${paymentModalInvoice?.invoiceNumber}`}
      >
          <form onSubmit={handleRecordPayment} className="space-y-4">
              <div className="bg-slate-50 p-3 rounded border mb-4">
                  <div className="flex justify-between text-sm">
                      <span className="text-slate-500">Total:</span>
                      <span className="font-bold">${paymentModalInvoice?.totalAmount.toLocaleString()}</span>
                  </div>
                  <div className="flex justify-between text-sm mt-1">
                      <span className="text-slate-500">Balance:</span>
                      <span className="font-bold text-rose-600">${paymentModalInvoice?.balanceAmount.toLocaleString()}</span>
                  </div>
              </div>

              <Input
                label="Payment Amount"
                type="number"
                step="0.01"
                required
                value={paymentData.amount}
                onChange={(e) => setPaymentData({ ...paymentData, amount: Number(e.target.value) })}
              />

              <div className="grid grid-cols-2 gap-4">
                  <Input
                    label="Payment Date"
                    type="date"
                    required
                    value={paymentData.paymentDate}
                    onChange={(e) => setPaymentData({ ...paymentData, paymentDate: e.target.value })}
                  />
                  <div className="space-y-2">
                      <label className="text-sm font-medium">Method</label>
                      <select 
                        className="w-full border rounded px-3 py-2 bg-background"
                        value={paymentData.method}
                        onChange={(e) => setPaymentData({ ...paymentData, method: Number(e.target.value) })}
                      >
                          <option value={0}>Bank Transfer</option>
                          <option value={1}>Credit Card</option>
                          <option value={2}>PayPal</option>
                          <option value={3}>Stripe</option>
                          <option value={4}>Cash</option>
                          <option value={5}>Other</option>
                      </select>
                  </div>
              </div>

              <Input
                label="Reference Number"
                placeholder="Check #, Transaction ID..."
                value={paymentData.referenceNumber}
                onChange={(e) => setPaymentData({ ...paymentData, referenceNumber: e.target.value })}
              />

              <Input
                label="Notes"
                placeholder="Optional payment notes..."
                value={paymentData.notes}
                onChange={(e) => setPaymentData({ ...paymentData, notes: e.target.value })}
              />

              <div className="flex justify-end gap-3 mt-6 pt-4 border-t">
                <Button variant="outline" type="button" onClick={() => setPaymentModalInvoice(null)} disabled={isRecordingPayment}>
                  Cancel
                </Button>
                <Button type="submit" isLoading={isRecordingPayment}>Record Payment</Button>
              </div>
          </form>
      </Modal>
    </Container>
  );
}
