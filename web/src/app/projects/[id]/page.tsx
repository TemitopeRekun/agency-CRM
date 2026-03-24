'use client';

import { useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useProjects } from '@/hooks/queries/useProjects';
import { useTasks } from '@/hooks/queries/useTasks';
import { useTimeEntries } from '@/hooks/queries/useTimeEntries';
import { useAdMetrics, AdPlatform } from '@/hooks/queries/useAdMetrics';
import { Container, Section } from '@/components/ui/LayoutPrimitives';
import { Button } from '@/components/ui/Button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Modal } from '@/components/ui/Modal';
import { Input } from '@/components/ui/Input';
import { toast } from 'sonner';

export default function ProjectDetailsPage() {
  const { id: projectId } = useParams() as { id: string };
  const { projects } = useProjects();
  const { tasks } = useTasks();
  const { entries, createEntry, isCreating: isLoggingTime } = useTimeEntries(projectId);
  const { metrics, createMetric, isCreating: isLoggingMetric } = useAdMetrics(projectId);
  
  const [activeTab, setActiveTab] = useState<'tasks' | 'time' | 'ads'>('tasks');
  const [isTimeModalOpen, setIsTimeModalOpen] = useState(false);
  const [isAdModalOpen, setIsAdModalOpen] = useState(false);
  
  const [newTime, setNewTime] = useState({ hours: 1, description: '', date: new Date().toISOString().split('T')[0] });
  const [newAd, setNewAd] = useState({ platform: AdPlatform.Google, spend: 0, impressions: 0, clicks: 0, conversions: 0, date: new Date().toISOString().split('T')[0] });

  const project = projects.find(p => p.id === projectId);

  if (!project) return <Container><Section>Project not found</Section></Container>;

  const handleLogTime = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await createEntry({ ...newTime, projectId, date: new Date(newTime.date).toISOString() });
      setIsTimeModalOpen(false);
      toast.success('Time logged successfully');
    } catch (err) {
      toast.error('Failed to log time');
    }
  };

  const handleLogAdMetric = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await createMetric({ ...newAd, projectId, date: new Date(newAd.date).toISOString() });
      setIsAdModalOpen(false);
      toast.success('Ad metrics saved');
    } catch (err) {
      toast.error('Failed to save metrics');
    }
  };

  return (
    <Container>
      <Section className="border-b pb-8">
        <h1 className="text-3xl font-bold">{project.name}</h1>
        <p className="text-muted-foreground mt-2 max-w-2xl">{project.description}</p>
      </Section>

      <div className="flex space-x-4 mt-8 bg-muted/30 p-1 rounded-lg w-fit">
        {(['tasks', 'time', 'ads'] as const).map(tab => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`px-4 py-2 rounded-md text-sm font-medium capitalize transition ${activeTab === tab ? 'bg-white shadow-sm text-blue-600' : 'text-muted-foreground hover:bg-white/50'}`}
          >
            {tab}
          </button>
        ))}
      </div>

      <div className="mt-8">
        {activeTab === 'tasks' && (
          <Section>
             <h2 className="text-xl font-semibold mb-4 text-emerald-700">Project Tasks</h2>
             <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Title</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Priority</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {tasks.filter(t => t.projectId === projectId).map(task => (
                    <TableRow key={task.id}>
                      <TableCell>{task.title}</TableCell>
                      <TableCell>{task.status}</TableCell>
                      <TableCell>{task.priority}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
             </Table>
          </Section>
        )}

        {activeTab === 'time' && (
          <Section>
            <div className="flex justify-between items-center mb-4">
               <h2 className="text-xl font-semibold text-blue-700">Time Logs</h2>
               <Button size="sm" onClick={() => setIsTimeModalOpen(true)}>Log Time</Button>
            </div>
            <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Date</TableHead>
                    <TableHead>Description</TableHead>
                    <TableHead className="text-right">Hours</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {entries.map(entry => (
                    <TableRow key={entry.id}>
                      <TableCell>{new Date(entry.date).toLocaleDateString()}</TableCell>
                      <TableCell>{entry.description}</TableCell>
                      <TableCell className="text-right font-bold">{entry.hours}h</TableCell>
                    </TableRow>
                  ))}
                  {entries.length === 0 && <TableRow><TableCell colSpan={3} className="text-center py-8 text-muted-foreground">No hours logged yet.</TableCell></TableRow>}
                </TableBody>
            </Table>
          </Section>
        )}

        {activeTab === 'ads' && (
          <Section>
            <div className="flex justify-between items-center mb-4">
               <h2 className="text-xl font-semibold text-rose-700">Advertising Performance</h2>
               <Button size="sm" onClick={() => setIsAdModalOpen(true)}>Add Metrics</Button>
            </div>
            <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Date</TableHead>
                    <TableHead>Platform</TableHead>
                    <TableHead className="text-right">Spend</TableHead>
                    <TableHead className="text-right">Impressions</TableHead>
                    <TableHead className="text-right">Conversions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {metrics.map(m => (
                    <TableRow key={m.id}>
                      <TableCell>{new Date(m.date).toLocaleDateString()}</TableCell>
                      <TableCell className="font-medium">{AdPlatform[m.platform]}</TableCell>
                      <TableCell className="text-right">${m.spend.toLocaleString()}</TableCell>
                      <TableCell className="text-right">{m.impressions.toLocaleString()}</TableCell>
                      <TableCell className="text-right font-bold text-emerald-600">{m.conversions}</TableCell>
                    </TableRow>
                  ))}
                  {metrics.length === 0 && <TableRow><TableCell colSpan={5} className="text-center py-8 text-muted-foreground">No ad performance data reported.</TableCell></TableRow>}
                </TableBody>
            </Table>
          </Section>
        )}
      </div>

      {/* Log Time Modal */}
      <Modal isOpen={isTimeModalOpen} onClose={() => setIsTimeModalOpen(false)} title="Log Work Hours">
         <form onSubmit={handleLogTime} className="space-y-4">
            <Input label="Hours" type="number" step="0.5" value={newTime.hours} onChange={e => setNewTime({ ...newTime, hours: Number(e.target.value) })} required />
            <Input label="Date" type="date" value={newTime.date} onChange={e => setNewTime({ ...newTime, date: e.target.value })} required />
            <Input label="Description" placeholder="Working on bug fixes..." value={newTime.description} onChange={e => setNewTime({ ...newTime, description: e.target.value })} required />
            <Button type="submit" className="w-full" isLoading={isLoggingTime}>Save Entry</Button>
         </form>
      </Modal>

      {/* Log Metrics Modal */}
      <Modal isOpen={isAdModalOpen} onClose={() => setIsAdModalOpen(false)} title="Log Ad Performance">
         <form onSubmit={handleLogAdMetric} className="space-y-4">
            <Input label="Date" type="date" value={newAd.date} onChange={e => setNewAd({ ...newAd, date: e.target.value })} required />
            <Input label="Spend ($)" type="number" value={newAd.spend} onChange={e => setNewAd({ ...newAd, spend: Number(e.target.value) })} required />
            <Input label="Impressions" type="number" value={newAd.impressions} onChange={e => setNewAd({ ...newAd, impressions: Number(e.target.value) })} required />
            <Input label="Conversions" type="number" value={newAd.conversions} onChange={e => setNewAd({ ...newAd, conversions: Number(e.target.value) })} required />
            <Button type="submit" className="w-full" isLoading={isLoggingMetric}>Save Metrics</Button>
         </form>
      </Modal>
    </Container>
  );
}
