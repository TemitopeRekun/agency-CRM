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
    quoteTemplateId: 'template-standard'
  });

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
        quoteTemplateId: formData.quoteTemplateId
      });
      toast.success('Offer created successfully');
      setFormData({ title: '', leadId: '', totalAmount: '', notes: '', quoteTemplateId: 'template-standard' });
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
      <form onSubmit={onSubmit} className="space-y-4">
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

        <div className="space-y-1.5">
          <label className="text-sm font-medium leading-none">Notes</label>
          <textarea
            className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
            placeholder="Additional details..."
            value={formData.notes}
            onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
          />
        </div>

        <div className="flex justify-end gap-3 pt-4">
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
