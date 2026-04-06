'use client';

import { useState } from 'react';
import { useLeads, LeadStatus, LeadSource, ServiceType, PipelineStage } from '@/hooks/queries/useLeads';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Container, Section } from '@/components/ui/LayoutPrimitives';
import { Modal } from '@/components/ui/Modal';

export default function LeadsPage() {
  const { leads, isLoading, createLead, isCreating, updateStatus, isUpdatingStatus } = useLeads();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [sortBy, setSortBy] = useState<'title' | 'date' | 'value'>('date');
  const [newLead, setNewLead] = useState({ 
    title: '', 
    description: '',
    contactName: '',
    email: '',
    phone: '',
    companyName: '',
    source: LeadSource.Manual,
    interest: ServiceType.Other,
    budgetRange: ''
  });

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await createLead(newLead);
      setNewLead({ 
        title: '', 
        description: '',
        contactName: '',
        email: '',
        phone: '',
        companyName: '',
        source: LeadSource.Manual,
        interest: ServiceType.Other,
        budgetRange: ''
      });
      setIsModalOpen(false);
    } catch (err) {
      console.error(err);
    }
  };

  const sortedLeads = [...leads].sort((a, b) => {
    if (sortBy === 'title') return a.title.localeCompare(b.title);
    if (sortBy === 'value') return (b.dealValue || 0) - (a.dealValue || 0);
    return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
  });

  return (
    <Container>
      <Section className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Leads</h1>
          <p className="text-muted-foreground mt-1">Manage your incoming opportunities and sales funnel.</p>
        </div>
        <div className="flex gap-3">
          <select 
            className="border rounded px-3 py-2 bg-background text-sm"
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value as any)}
          >
            <option value="date">Sort by Date</option>
            <option value="title">Sort by Name</option>
            <option value="value">Sort by Value</option>
          </select>
          <Button onClick={() => setIsModalOpen(true)}>Add Lead</Button>
        </div>
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
                <TableHead>Lead / Company</TableHead>
                <TableHead>Contact</TableHead>
                <TableHead>Source</TableHead>
                <TableHead>Interest</TableHead>
                <TableHead>Value</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {sortedLeads.map((l) => (
                <TableRow key={l.id}>
                  <TableCell>
                    <div className="font-medium">{l.title}</div>
                    <div className="text-xs text-muted-foreground">{l.companyName}</div>
                  </TableCell>
                  <TableCell>
                    <div className="text-sm">{l.contactName}</div>
                    <div className="text-xs text-muted-foreground">{l.email}</div>
                  </TableCell>
                  <TableCell>
                    <span className="text-xs border px-2 py-0.5 rounded bg-muted">
                        {LeadSource[l.source]}
                    </span>
                  </TableCell>
                  <TableCell>
                    <span className="text-xs">
                        {ServiceType[l.interest]}
                    </span>
                  </TableCell>
                  <TableCell>
                    <div className="font-medium text-emerald-600">
                        {l.dealValue ? `$${l.dealValue.toLocaleString()}` : '-'}
                    </div>
                    <div className="text-[10px] text-muted-foreground">Prob: {l.probability}%</div>
                  </TableCell>
                  <TableCell>
                    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                        l.status === LeadStatus.Qualified ? 'bg-green-100 text-green-800' :
                        l.status === LeadStatus.Lost ? 'bg-red-100 text-red-800' :
                        'bg-indigo-100 text-indigo-800'
                    }`}>
                      {LeadStatus[l.status]}
                    </span>
                  </TableCell>
                  <TableCell className="text-right">
                    <select
                      className="border rounded px-2 py-1 text-sm bg-background"
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
                  <TableCell colSpan={7} className="text-center text-muted-foreground py-12">
                    <div className="text-lg font-medium">No leads found</div>
                    <p className="text-sm">Start by adding your first sales opportunity.</p>
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
          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Deal Title"
              placeholder="e.g. Website Redesign"
              value={newLead.title}
              onChange={(e) => setNewLead({ ...newLead, title: e.target.value })}
              required
            />
            <Input
              label="Company Name"
              placeholder="Acme Corp"
              value={newLead.companyName}
              onChange={(e) => setNewLead({ ...newLead, companyName: e.target.value })}
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Contact Name"
              placeholder="John Doe"
              value={newLead.contactName}
              onChange={(e) => setNewLead({ ...newLead, contactName: e.target.value })}
              required
            />
            <Input
              label="Email Address"
              type="email"
              placeholder="john@example.com"
              value={newLead.email}
              onChange={(e) => setNewLead({ ...newLead, email: e.target.value })}
              required
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
                <label className="text-sm font-medium">Source</label>
                <select 
                    className="w-full border rounded px-3 py-2 bg-background"
                    value={newLead.source}
                    onChange={(e) => setNewLead({ ...newLead, source: parseInt(e.target.value) })}
                >
                    <option value={LeadSource.Facebook}>Facebook</option>
                    <option value={LeadSource.Google}>Google</option>
                    <option value={LeadSource.Website}>Website</option>
                    <option value={LeadSource.Referral}>Referral</option>
                    <option value={LeadSource.Manual}>Manual</option>
                </select>
            </div>
            <div className="space-y-2">
                <label className="text-sm font-medium">Service Interest</label>
                <select 
                    className="w-full border rounded px-3 py-2 bg-background"
                    value={newLead.interest}
                    onChange={(e) => setNewLead({ ...newLead, interest: parseInt(e.target.value) })}
                >
                    <option value={ServiceType.Development}>Development</option>
                    <option value={ServiceType.Marketing}>Marketing</option>
                    <option value={ServiceType.Staffing}>Staffing</option>
                    <option value={ServiceType.Other}>Other</option>
                </select>
            </div>
          </div>

          <Input
            label="Budget Range / Notes"
            placeholder="$5k - $10k"
            value={newLead.budgetRange}
            onChange={(e) => setNewLead({ ...newLead, budgetRange: e.target.value })}
          />

          <Input
            label="Internal Description"
            placeholder="Key requirements or pitch notes..."
            value={newLead.description}
            onChange={(e) => setNewLead({ ...newLead, description: e.target.value })}
          />

          <div className="flex justify-end gap-3 mt-6 pt-4 border-t">
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
