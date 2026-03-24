'use client';

import { useState } from 'react';
import { useContracts, ContractStatus } from '@/hooks/queries/useContracts';
import { useProjects } from '@/hooks/queries/useProjects';
import { useInvoices } from '@/hooks/queries/useInvoices';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Container, Section } from '@/components/ui/LayoutPrimitives';
import { Button } from '@/components/ui/Button';
import { toast } from 'sonner';

export default function ContractsPage() {
  const { contracts, isLoading } = useContracts();
  const { projects } = useProjects();
  const { generateFromContract, isGeneratingFromContract } = useInvoices();
  const [generatingId, setGeneratingId] = useState<string | null>(null);

  const handleGenerateInvoice = async (contractId: string) => {
    setGeneratingId(contractId);
    try {
      await generateFromContract(contractId);
      toast.success('Invoiced generated from contract');
    } catch (err) {
      console.error(err);
      toast.error('Failed to generate invoice');
    } finally {
      setGeneratingId(null);
    }
  };

  const getProjectName = (projectId: string) => {
    const project = projects.find(p => p.id === projectId);
    return project ? project.name : 'Unknown Project';
  };

  return (
    <Container>
      <Section className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Contracts</h1>
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
                <TableHead>Contract Title</TableHead>
                <TableHead>Project</TableHead>
                <TableHead>Amount</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Created At</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {contracts.map((c) => (
                <TableRow key={c.id}>
                  <TableCell className="font-medium">{c.title}</TableCell>
                  <TableCell>{getProjectName(c.projectId)}</TableCell>
                  <TableCell className="font-medium text-emerald-600">
                    ${c.totalAmount?.toLocaleString() || '0'}
                  </TableCell>
                  <TableCell>
                    <span className="inline-flex items-center rounded-full bg-blue-100 px-2.5 py-0.5 text-xs font-medium text-blue-800">
                      {ContractStatus[c.status]}
                    </span>
                  </TableCell>
                  <TableCell>{new Date(c.createdAt).toLocaleDateString()}</TableCell>
                  <TableCell className="text-right">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleGenerateInvoice(c.id)}
                      isLoading={generatingId === c.id}
                      disabled={isGeneratingFromContract}
                    >
                      Invoice
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {contracts.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6} className="text-center text-muted-foreground py-8">
                    No contracts found. Generate one from a Project.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        )}
      </Section>
    </Container>
  );
}
