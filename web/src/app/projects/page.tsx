'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useProjects } from '@/hooks/queries/useProjects';
import { useClients } from '@/hooks/queries/useClients';
import { useContracts } from '@/hooks/queries/useContracts';
import { useInvoices } from '@/hooks/queries/useInvoices';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Container, Section } from '@/components/ui/LayoutPrimitives';
import { Modal } from '@/components/ui/Modal';
import { toast } from 'sonner';

export default function ProjectsPage() {
  const { projects, isLoading, createProject, isCreating } = useProjects();
  const { clients } = useClients();
  const { generateContract, isGenerating } = useContracts();
  const { generateFromProject, isGeneratingFromProject } = useInvoices();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newProject, setNewProject] = useState({ name: '', description: '', clientId: '' });
  const [generatingId, setGeneratingId] = useState<string | null>(null);
  const [generatingInvoiceId, setGeneratingInvoiceId] = useState<string | null>(null);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await createProject(newProject);
      setNewProject({ name: '', description: '', clientId: '' });
      setIsModalOpen(false);
      toast.success('Project created successfully');
    } catch (err) {
      console.error(err);
      toast.error('Failed to create project');
    }
  };

  const handleGenerateContract = async (projectId: string) => {
    setGeneratingId(projectId);
    try {
      await generateContract(projectId);
      toast.success('Contract generated successfully');
    } catch (err) {
      console.error(err);
      toast.error('Failed to generate contract');
    } finally {
      setGeneratingId(null);
    }
  };

  const handleGenerateInvoice = async (projectId: string) => {
    setGeneratingInvoiceId(projectId);
    try {
      await generateFromProject(projectId);
      toast.success('Draft invoice generated successfully');
    } catch (err) {
      console.error(err);
      toast.error('Failed to generate invoice');
    } finally {
      setGeneratingInvoiceId(null);
    }
  };

  const getClientName = (clientId?: string) => {
    if (!clientId) return 'No Client';
    const client = clients.find(c => c.id === clientId);
    return client ? client.name : 'Unknown Client';
  };

  const clientOptions = [
    { label: 'Select a Client', value: '' },
    ...clients.map(c => ({ label: c.name, value: c.id }))
  ];

  return (
    <Container>
      <Section className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Projects</h1>
        <Button onClick={() => setIsModalOpen(true)}>Add Project</Button>
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
                <TableHead>Project Name</TableHead>
                <TableHead>Client</TableHead>
                <TableHead>Description</TableHead>
                <TableHead>Created At</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {projects.map((p) => (
                <TableRow key={p.id}>
                  <TableCell className="font-medium">
                    <Link href={`/projects/${p.id}`} className="text-blue-600 hover:underline">
                      {p.name}
                    </Link>
                  </TableCell>
                  <TableCell>{getClientName(p.clientId)}</TableCell>
                  <TableCell className="max-w-md truncate">{p.description}</TableCell>
                  <TableCell>{new Date(p.createdAt).toLocaleDateString()}</TableCell>
                  <TableCell className="text-right space-x-2">
                    <Button 
                      variant="outline" 
                      onClick={() => handleGenerateContract(p.id)} 
                      isLoading={generatingId === p.id}
                      disabled={isGenerating || isGeneratingFromProject}
                      className="text-amber-600 border-amber-200 hover:bg-amber-50"
                    >
                      Contract
                    </Button>
                    <Button 
                      variant="outline" 
                      onClick={() => handleGenerateInvoice(p.id)} 
                      isLoading={generatingInvoiceId === p.id}
                      disabled={isGenerating || isGeneratingFromProject}
                      className="text-emerald-600 border-emerald-200 hover:bg-emerald-50"
                    >
                      Invoice
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {projects.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5} className="text-center text-muted-foreground py-8">
                    No projects found.
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
        title="Create New Project"
      >
        <form onSubmit={handleCreate} className="space-y-4">
          <Input
            label="Project Name"
            placeholder="Website Redesign"
            value={newProject.name}
            onChange={(e) => setNewProject({ ...newProject, name: e.target.value })}
            required
          />
          <Select
            label="Client"
            options={clientOptions}
            value={newProject.clientId}
            onChange={(e) => setNewProject({ ...newProject, clientId: e.target.value })}
            required
          />
          <div className="space-y-1.5">
            <label className="text-sm font-medium leading-none">Description</label>
            <textarea
              className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              placeholder="Project details..."
              value={newProject.description}
              onChange={(e) => setNewProject({ ...newProject, description: e.target.value })}
              required
            />
          </div>

          <div className="flex justify-end gap-3 mt-6">
            <Button variant="outline" type="button" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button type="submit" isLoading={isCreating}>Create Project</Button>
          </div>
        </form>
      </Modal>
    </Container>
  );
}
