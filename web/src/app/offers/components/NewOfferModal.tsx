'use client';

import { useState } from 'react';
import { Modal } from '@/components/ui/Modal';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { useLeads } from '@/hooks/queries/useLeads';
import { useOffers } from '@/hooks/queries/useOffers';
import { toast } from 'sonner';

interface NewOfferModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const NewOfferModal = ({ isOpen, onClose }: NewOfferModalProps) => {
  const { leads, isLoading: leadsLoading } = useLeads();
  const { createOffer, isCreating } = useOffers();
  
  const [formData, setFormData] = useState({
    title: '',
    leadId: '',
    totalAmount: '',
    notes: '',
    quoteTemplateId: 'template-standard',
    items: [] as { title: string; description: string; amount: number }[]
  });

  const addItem = () => {
    setFormData({
      ...formData,
      items: [...formData.items, { title: '', description: '', amount: 0 }]
    });
  };

  const removeItem = (index: number) => {
    const newItems = [...formData.items];
    newItems.splice(index, 1);
    setFormData({ ...formData, items: newItems });
  };

  const updateItem = (index: number, field: string, value: any) => {
    const newItems = [...formData.items];
    newItems[index] = { ...newItems[index], [field]: value };
    
    // Auto-calculate total amount if items exist
    const total = newItems.reduce((sum, item) => sum + Number(item.amount), 0);
    setFormData({ 
      ...formData, 
      items: newItems,
      totalAmount: total > 0 ? total.toString() : formData.totalAmount 
    });
  };

  const [errors, setErrors] = useState<{ [key: string]: string }>({});

  const validate = () => {
    const newErrors: { [key: string]: string } = {};
    if (!formData.title) newErrors.title = 'Title is required';
    if (!formData.leadId) newErrors.leadId = 'Lead is required';
    if (!formData.totalAmount || Number(formData.totalAmount) < 0) {
      newErrors.totalAmount = 'Valid amount is required';
    }
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      await createOffer({
        title: formData.title,
        leadId: formData.leadId,
        totalAmount: Number(formData.totalAmount),
        notes: formData.notes,
        quoteTemplateId: formData.quoteTemplateId,
        items: formData.items
      });
      toast.success('Offer created successfully');
      setFormData({ 
        title: '', 
        leadId: '', 
        totalAmount: '', 
        notes: '', 
        quoteTemplateId: 'template-standard',
        items: []
      });
      onClose();
    } catch (err) {
      console.error(err);
      toast.error('Failed to create offer');
    }
  };

  const leadOptions = [
    { label: 'Select a Lead', value: '' },
    ...leads.map(lead => ({
      label: lead.title,
      value: lead.id
    }))
  ];

  return (
    <Modal 
      isOpen={isOpen} 
      onClose={onClose} 
      title="Create New Offer"
    >
      <form onSubmit={onSubmit} className="space-y-4 max-h-[80vh] overflow-y-auto pr-2">
        <div className="grid grid-cols-2 gap-4">
            <Input
            label="Offer Title"
            placeholder="e.g. Website Overhaul"
            value={formData.title}
            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
            error={errors.title}
            />

            <Select
            label="Select Lead"
            options={leadOptions}
            disabled={leadsLoading}
            value={formData.leadId}
            onChange={(e) => setFormData({ ...formData, leadId: e.target.value })}
            error={errors.leadId}
            />
        </div>

        <div className="border rounded-lg p-4 bg-slate-50/50 space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-sm font-semibold">Breakdown (Phases / Services)</h3>
                <Button type="button" size="sm" variant="outline" onClick={addItem}>
                    + Add Item
                </Button>
            </div>
            
            {formData.items.map((item, idx) => (
                <div key={idx} className="grid grid-cols-12 gap-2 items-start bg-background p-3 rounded border shadow-sm">
                    <div className="col-span-5">
                        <Input
                            placeholder="Title (e.g. Design)"
                            value={item.title}
                            onChange={(e) => updateItem(idx, 'title', e.target.value)}
                            required
                        />
                    </div>
                    <div className="col-span-4">
                        <Input
                            placeholder="Amount ($)"
                            type="number"
                            value={item.amount}
                            onChange={(e) => updateItem(idx, 'amount', e.target.value)}
                            required
                        />
                    </div>
                    <div className="col-span-12 mt-2">
                        <textarea
                            className="w-full text-xs p-2 border rounded bg-slate-50"
                            placeholder="Brief description of this phase..."
                            value={item.description}
                            onChange={(e) => updateItem(idx, 'description', e.target.value)}
                        />
                    </div>
                    <div className="col-start-12 col-span-1 pt-1">
                        <button 
                            type="button" 
                            onClick={() => removeItem(idx)}
                            className="text-rose-500 hover:text-rose-700"
                        >
                            ×
                        </button>
                    </div>
                </div>
            ))}

            {formData.items.length === 0 && (
                <p className="text-xs text-center text-muted-foreground py-4">
                    No items added yet. Add items for a multi-phase breakdown.
                </p>
            )}
        </div>

        <div className="grid grid-cols-2 gap-4">
            <Select
            label="Quote Template"
            options={[
                { label: 'Standard Digital Marketing', value: 'template-standard' },
                { label: 'Premium Web Dev', value: 'template-premium' },
                { label: 'Custom', value: 'template-custom' }
            ]}
            value={formData.quoteTemplateId}
            onChange={(e) => setFormData({ ...formData, quoteTemplateId: e.target.value })}
            />

            <Input
            label="Total Amount ($)"
            type="number"
            step="0.01"
            placeholder="0.00"
            value={formData.totalAmount}
            onChange={(e) => setFormData({ ...formData, totalAmount: e.target.value })}
            error={errors.totalAmount}
            />
        </div>

        <div className="space-y-1.5">
          <label className="text-sm font-medium leading-none">Internal Notes</label>
          <textarea
            className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
            placeholder="Additional internal details..."
            value={formData.notes}
            onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
          />
        </div>

        <div className="flex justify-end gap-3 pt-4 border-t">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isCreating}>
            Create Offer
          </Button>
        </div>
      </form>
    </Modal>
  );
};
