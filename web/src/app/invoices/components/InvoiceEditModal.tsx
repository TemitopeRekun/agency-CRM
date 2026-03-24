'use client';

import { useState, useEffect } from 'react';
import { Modal } from '@/components/ui/Modal';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { useInvoices, Invoice, InvoiceItem } from '@/hooks/queries/useInvoices';
import { toast } from 'sonner';

interface InvoiceEditModalProps {
  isOpen: boolean;
  onClose: () => void;
  invoice: Invoice | null;
}

export const InvoiceEditModal = ({ isOpen, onClose, invoice }: InvoiceEditModalProps) => {
  const { updateInvoice, isUpdatingInvoice } = useInvoices();
  
  const [items, setItems] = useState<Partial<InvoiceItem>[]>([]);
  const [dueDate, setDueDate] = useState('');

  useEffect(() => {
    if (invoice) {
        setItems(invoice.items.length ? [...invoice.items] : [{ description: '', quantity: 1, unitPrice: 0 }]);
        setDueDate(new Date(invoice.dueDate).toISOString().split('T')[0]);
    }
  }, [invoice]);

  const validate = () => {
    if (!dueDate) return false;
    for (const item of items) {
        if (!item.description || item.quantity === undefined || item.unitPrice === undefined) return false;
    }
    return true;
  };

  const handleAddItem = () => {
      setItems([...items, { description: '', quantity: 1, unitPrice: 0 }]);
  };

  const handleRemoveItem = (index: number) => {
      setItems(items.filter((_, i) => i !== index));
  };

  const calculateTotal = () => {
      return items.reduce((sum, item) => sum + ((item.quantity || 0) * (item.unitPrice || 0)), 0);
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate() || !invoice) return;

    try {
      await updateInvoice({
        id: invoice.id,
        data: {
            totalAmount: calculateTotal(),
            dueDate: new Date(dueDate).toISOString(),
            status: invoice.status,
            items: items as InvoiceItem[]
        }
      });
      toast.success('Invoice updated successfully');
      onClose();
    } catch (err) {
      console.error(err);
      toast.error('Failed to update invoice');
    }
  };

  if (!invoice) return null;

  return (
    <Modal 
      isOpen={isOpen} 
      onClose={onClose} 
      title={`Edit Invoice ${invoice.invoiceNumber}`}
    >
      <form onSubmit={onSubmit} className="space-y-4 max-h-[70vh] overflow-y-auto p-1">
        
        <Input
          label="Due Date"
          type="date"
          value={dueDate}
          onChange={(e) => setDueDate(e.target.value)}
          required
        />

        <div className="space-y-2">
            <div className="flex justify-between items-center">
                <label className="text-sm font-medium">Line Items</label>
                <Button type="button" variant="outline" size="sm" onClick={handleAddItem}>+ Add Item</Button>
            </div>
            
            {items.map((item, index) => (
                <div key={index} className="flex gap-2 items-start bg-muted/30 p-2 rounded border">
                    <div className="flex-1 space-y-2">
                        <Input
                            placeholder="Description"
                            value={item.description}
                            onChange={(e) => {
                                const newItems = [...items];
                                newItems[index].description = e.target.value;
                                setItems(newItems);
                            }}
                            required
                        />
                        <div className="flex gap-2">
                            <Input
                                type="number"
                                placeholder="Qty"
                                min="0.1"
                                step="0.1"
                                value={item.quantity}
                                onChange={(e) => {
                                    const newItems = [...items];
                                    newItems[index].quantity = Number(e.target.value);
                                    setItems(newItems);
                                }}
                                required
                            />
                            <Input
                                type="number"
                                placeholder="Unit Price"
                                step="0.01"
                                value={item.unitPrice}
                                onChange={(e) => {
                                    const newItems = [...items];
                                    newItems[index].unitPrice = Number(e.target.value);
                                    setItems(newItems);
                                }}
                                required
                            />
                        </div>
                    </div>
                    {items.length > 1 && (
                        <Button type="button" variant="destructive" size="sm" onClick={() => handleRemoveItem(index)}>X</Button>
                    )}
                </div>
            ))}
        </div>

        <div className="flex justify-between items-center pt-4 border-t">
            <span className="font-bold">Total: ${calculateTotal().toFixed(2)}</span>
            <div className="flex gap-3">
                <Button type="button" variant="outline" onClick={onClose}>
                    Cancel
                </Button>
                <Button type="submit" isLoading={isUpdatingInvoice}>
                    Save Changes
                </Button>
            </div>
        </div>
      </form>
    </Modal>
  );
};
