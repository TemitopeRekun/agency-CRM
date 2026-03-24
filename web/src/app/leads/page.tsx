'use client';

import { useState } from 'react';
import { useLeads, LeadStatus } from '@/hooks/queries/useLeads';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Container, Section } from '@/components/ui/LayoutPrimitives';
import { Modal } from '@/components/ui/Modal';

export default function LeadsPage() {
  const { leads, isLoading, createLead, isCreating, updateStatus, isUpdatingStatus } = useLeads();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newLead, setNewLead] = useState({ title: '', description: '' });

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await createLead(newLead);
      setNewLead({ title: '', description: '' });
      setIsModalOpen(false);
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <Container>
      <Section className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Leads</h1>
        <Button onClick={() => setIsModalOpen(true)}>Add Lead</Button>
      </Section>

      <Section>
        {isLoading ? (
          <div className="space-y-4">
            {[1, 2, 3, 4].map((i) => (
              <div key={i} className="h-12 w-full bg-muted animate-pulse rounded" />
            ))}
          </div>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Title</TableHead>
                <TableHead>Description</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {leads.map((l) => (
                <TableRow key={l.id}>
                  <TableCell className="font-medium">{l.title}</TableCell>
                  <TableCell>{l.description}</TableCell>
                  <TableCell>
                    <span className="inline-flex items-center rounded-full bg-indigo-100 px-2.5 py-0.5 text-xs font-medium text-indigo-800">
                      {LeadStatus[l.status]}
                    </span>
                  </TableCell>
                  <TableCell className="text-right">
                    <select
                      className="border rounded px-2 py-1 text-sm bg-background mr-2"
                      value={l.status}
                      disabled={isUpdatingStatus}
                      onChange={(e) => updateStatus({ id: l.id, status: parseInt(e.target.value) as LeadStatus })}
                    >
                      <option value={LeadStatus.New}>New</option>
                      <option value={LeadStatus.Contacted}>Contacted</option>
                      <option value={LeadStatus.Qualified}>Qualified</option>
                      <option value={LeadStatus.Lost}>Lost</option>
                    </select>
                  </TableCell>
                </TableRow>
              ))}
              {leads.length === 0 && (
                <TableRow>
                  <TableCell colSpan={4} className="text-center text-muted-foreground py-8">
                    No leads found.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        )}
      </Section>

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="Create New Lead"
      >
        <form onSubmit={handleCreate} className="space-y-4">
          <Input
            label="Lead Title (Company / Person)"
            placeholder="Acme Corp"
            value={newLead.title}
            onChange={(e) => setNewLead({ ...newLead, title: e.target.value })}
            required
          />
          <Input
            label="Description / Pitch"
            placeholder="Interested in our services..."
            value={newLead.description}
            onChange={(e) => setNewLead({ ...newLead, description: e.target.value })}
            required
          />

          <div className="flex justify-end gap-3 mt-6">
            <Button variant="outline" type="button" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button type="submit" isLoading={isCreating}>Create Lead</Button>
          </div>
        </form>
      </Modal>
    </Container>
  );
}
