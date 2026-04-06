'use client';

import { useState } from 'react';
import { useClients, PriorityTier, Client } from '@/hooks/queries/useClients';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Container, Section } from '@/components/ui/LayoutPrimitives';
import { Modal } from '@/components/ui/Modal';

export default function ClientsPage() {
  const { clients, isLoading, createClient, isCreating } = useClients();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [sortBy, setSortBy] = useState<'name' | 'priority' | 'date'>('date');
  const [newClient, setNewClient] = useState({ 
    name: '', 
    legalName: '',
    vatNumber: '',
    businessAddress: '',
    industry: '',
    priority: PriorityTier.Tier3
  });

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await createClient(newClient);
      setNewClient({ 
        name: '', 
        legalName: '',
        vatNumber: '',
        businessAddress: '',
        industry: '',
        priority: PriorityTier.Tier3
      });
      setIsModalOpen(false);
    } catch (err) {
      console.error(err);
    }
  };

  const sortedClients = [...clients].sort((a, b) => {
    if (sortBy === 'name') return a.name.localeCompare(b.name);
    if (sortBy === 'priority') return a.priority - b.priority;
    return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
  });

  return (
    <Container>
      <Section className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Clients</h1>
          <p className="text-muted-foreground mt-1">Manage your active accounts and business relationships.</p>
        </div>
        <div className="flex gap-3">
          <select 
            className="border rounded px-3 py-2 bg-background text-sm"
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value as any)}
          >
            <option value="date">Sort by Date</option>
            <option value="name">Sort by Name</option>
            <option value="priority">Sort by Priority</option>
          </select>
          <Button onClick={() => setIsModalOpen(true)}>Add Client</Button>
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
                <TableHead>Client / Legal Name</TableHead>
                <TableHead>Industry</TableHead>
                <TableHead>Tier</TableHead>
                <TableHead>VAT Number</TableHead>
                <TableHead className="text-right">Created</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {sortedClients.map((c) => (
                <TableRow key={c.id}>
                  <TableCell>
                    <div className="font-medium">{c.name}</div>
                    <div className="text-xs text-muted-foreground">{c.legalName}</div>
                  </TableCell>
                  <TableCell>
                    <span className="text-sm">{c.industry || '-'}</span>
                  </TableCell>
                  <TableCell>
                    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                        c.priority === PriorityTier.Tier1 ? 'bg-purple-100 text-purple-800' :
                        c.priority === PriorityTier.Tier2 ? 'bg-blue-100 text-blue-800' :
                        'bg-gray-100 text-gray-800'
                    }`}>
                        {PriorityTier[c.priority]}
                    </span>
                  </TableCell>
                  <TableCell>
                    <code className="text-xs">{c.vatNumber || '-'}</code>
                  </TableCell>
                  <TableCell className="text-right text-xs text-muted-foreground">
                    {new Date(c.createdAt).toLocaleDateString()}
                  </TableCell>
                </TableRow>
              ))}
              {clients.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5} className="text-center text-muted-foreground py-12">
                    <div className="text-lg font-medium">No clients found</div>
                    <p className="text-sm">Active leads will appear here once converted.</p>
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
        title="Create New Client"
      >
        <form onSubmit={handleCreate} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Client Display Name"
              placeholder="e.g. Acme Mobile"
              value={newClient.name}
              onChange={(e) => setNewClient({ ...newClient, name: e.target.value })}
              required
            />
            <Input
              label="Legal Entity Name"
              placeholder="Acme Solutions LTD"
              value={newClient.legalName}
              onChange={(e) => setNewClient({ ...newClient, legalName: e.target.value })}
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="VAT / Tax ID"
              placeholder="GB123456789"
              value={newClient.vatNumber}
              onChange={(e) => setNewClient({ ...newClient, vatNumber: e.target.value })}
            />
            <div className="space-y-2">
                <label className="text-sm font-medium">Priority Tier</label>
                <select 
                    className="w-full border rounded px-3 py-2 bg-background"
                    value={newClient.priority}
                    onChange={(e) => setNewClient({ ...newClient, priority: parseInt(e.target.value) })}
                >
                    <option value={PriorityTier.Tier1}>Tier 1 (High)</option>
                    <option value={PriorityTier.Tier2}>Tier 2 (Medium)</option>
                    <option value={PriorityTier.Tier3}>Tier 3 (Low)</option>
                </select>
            </div>
          </div>

          <Input
            label="Industry"
            placeholder="e.g. FinTech, Healthcare"
            value={newClient.industry}
            onChange={(e) => setNewClient({ ...newClient, industry: e.target.value })}
          />

          <Input
            label="Business Address"
            placeholder="123 Business St, London, UK"
            value={newClient.businessAddress}
            onChange={(e) => setNewClient({ ...newClient, businessAddress: e.target.value })}
          />

          <div className="flex justify-end gap-3 mt-6 pt-4 border-t">
            <Button variant="outline" type="button" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button type="submit" isLoading={isCreating}>Create Client</Button>
          </div>
        </form>
      </Modal>
    </Container>
  );
}
