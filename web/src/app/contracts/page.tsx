'use client';

import { useState } from 'react';
import { useContracts, ContractStatus, SuccessFeeType } from '@/hooks/queries/useContracts';
import { useProjects } from '@/hooks/queries/useProjects';
import { useInvoices } from '@/hooks/queries/useInvoices';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Container, Section } from '@/components/ui/LayoutPrimitives';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { toast } from 'sonner';

export default function ContractsPage() {
  const { contracts, isLoading, signContract, isSigning, createContract, isCreating } = useContracts();
  const { projects } = useProjects();
  const { generateFromContract, isGeneratingFromContract } = useInvoices();
  const [generatingId, setGeneratingId] = useState<string | null>(null);
  const [signingId, setSigningId] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [formData, setFormData] = useState({
    title: '',
    totalAmount: 0,
    projectId: '',
    terms: 'Standard Agency Terms.',
    kpis: '1. Growth milestones\n2. ROI targets',
    baseRetainer: 0,
    successFeeType: SuccessFeeType.None,
    successFeeValue: 0
  });

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await createContract(formData);
      toast.success('Contract created with billing terms');
      setIsModalOpen(false);
      setFormData({
        title: '',
        totalAmount: 0,
        projectId: '',
        terms: 'Standard Agency Terms.',
        kpis: '1. Growth milestones\n2. ROI targets',
        baseRetainer: 0,
        successFeeType: SuccessFeeType.None,
        successFeeValue: 0
      });
    } catch (err) {
      console.error(err);
      toast.error('Failed to create contract');
    }
  };

  const handleSignContract = async (contractId: string) => {
    setSigningId(contractId);
    try {
      await signContract({ id: contractId, digitalSignature: "Internal Signed (Admin)" });
      toast.success('Contract signed successfully');
    } catch (err) {
      console.error(err);
      toast.error('Failed to sign contract');
    } finally {
      setSigningId(null);
    }
  };

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
        <h1 className="text-3xl font-bold tracking-tight">Contracts & Billing</h1>
        <Button onClick={() => setIsModalOpen(true)}>New Contract</Button>
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
                <TableHead>Amount (Total)</TableHead>
                <TableHead>Billing Terms</TableHead>
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
                    <div className="flex flex-col text-xs">
                      <span className="font-semibold text-slate-700">Retainer: ${c.baseRetainer?.toLocaleString() || '0'}</span>
                      <span className="text-muted-foreground italic">
                        {c.successFeeType === SuccessFeeType.FixedPerLead ? `Bonus: $${c.successFeeValue}/lead` : 
                         c.successFeeType === SuccessFeeType.RevenueShare ? `Bonus: ${c.successFeeValue}% Share` : 
                         'No Success Fee'}
                      </span>
                    </div>
                  </TableCell>
                  <TableCell>
                    <div className="flex flex-col gap-1">
                      <span className="inline-flex items-center rounded-full bg-blue-100 px-2.5 py-0.5 text-xs font-medium text-blue-800 w-fit">
                        {ContractStatus[c.status]}
                      </span>
                      {c.status === ContractStatus.Signed ? (
                        <span className="text-xs text-green-600 font-medium">Signed (v{c.version || 1})</span>
                      ) : (
                        <span className="text-xs text-amber-600 font-medium">Pending (v{c.version || 1})</span>
                      )}
                    </div>
                  </TableCell>
                  <TableCell>{new Date(c.createdAt).toLocaleDateString()}</TableCell>
                  <TableCell className="text-right flex gap-2 justify-end items-center h-full">
                    {c.status !== ContractStatus.Signed && (
                      <Button
                        variant="primary"
                        size="sm"
                        onClick={() => handleSignContract(c.id)}
                        isLoading={signingId === c.id}
                        disabled={isSigning}
                      >
                        Sign (Internal)
                      </Button>
                    )}
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
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="Draft New Agency Contract"
      >
        <form onSubmit={handleCreate} className="space-y-4">
          <Input 
            label="Contract Title" 
            value={formData.title} 
            onChange={(e) => setFormData({...formData, title: e.target.value})} 
            required 
          />
          <Select
            label="Project"
            value={formData.projectId}
            onChange={(e) => setFormData({...formData, projectId: e.target.value})}
            options={[
              { label: 'Select Project', value: '' },
              ...projects.map(p => ({ label: p.name, value: p.id }))
            ]}
            required
          />
          
          <div className="grid grid-cols-2 gap-4 border-t pt-4">
            <Input 
              label="Base Retainer ($)" 
              type="number" 
              value={formData.baseRetainer} 
              onChange={(e) => setFormData({...formData, baseRetainer: parseFloat(e.target.value)})} 
            />
            <Input 
              label="Contract Value ($)" 
              type="number" 
              value={formData.totalAmount} 
              onChange={(e) => setFormData({...formData, totalAmount: parseFloat(e.target.value)})} 
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Select
              label="Success Fee Type"
              value={formData.successFeeType.toString()}
              onChange={(e) => setFormData({...formData, successFeeType: parseInt(e.target.value)})}
              options={[
                { label: 'No Success Fee', value: SuccessFeeType.None.toString() },
                { label: 'Fixed per Lead', value: SuccessFeeType.FixedPerLead.toString() },
                { label: 'Revenue Share (%)', value: SuccessFeeType.RevenueShare.toString() }
              ]}
            />
            <Input 
              label="Success Fee Value" 
              type="number" 
              placeholder={formData.successFeeType === SuccessFeeType.RevenueShare ? "e.g. 10 for 10%" : "e.g. 50 for $50/lead"}
              value={formData.successFeeValue} 
              onChange={(e) => setFormData({...formData, successFeeValue: parseFloat(e.target.value)})} 
              disabled={formData.successFeeType === SuccessFeeType.None}
            />
          </div>

          <div className="flex justify-end gap-3 mt-6">
            <Button variant="outline" type="button" onClick={() => setIsModalOpen(false)}>Cancel</Button>
            <Button type="submit" isLoading={isCreating}>Create Draft</Button>
          </div>
        </form>
      </Modal>
    </Container>
  );
}
