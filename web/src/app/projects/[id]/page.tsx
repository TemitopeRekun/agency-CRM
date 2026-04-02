'use client';

import { useState } from 'react';
import { useParams } from 'next/navigation';
import { useProjects } from '@/hooks/queries/useProjects';
import { useTasks } from '@/hooks/queries/useTasks';
import { useTimeTracking } from '@/hooks/queries/useTimeTracking';
import { useAdMetrics, AdPlatform } from '@/hooks/queries/useAdMetrics';
import { useAdAccounts } from '@/hooks/queries/useAdAccounts';
import { Container, Section } from '@/components/ui/LayoutPrimitives';
import { Button } from '@/components/ui/Button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Modal } from '@/components/ui/Modal';
import { Input } from '@/components/ui/Input';
import { GanttChart } from '@/components/ui/GanttChart';
import { toast } from 'sonner';

export default function ProjectDetailsPage() {
  const { id: projectId } = useParams() as { id: string };
  const { projects } = useProjects();
  const { tasks } = useTasks();
  const { 
    timeEntries, 
    projectTeam, 
    logTime, 
    isLoggingTime
  } = useTimeTracking(projectId);
  const { metrics, createMetric, isCreating: isLoggingMetric, analytics: projectAnalytics } = useAdMetrics(projectId);
  const { accounts, linkAccount, unlinkAccount, sync, isSyncing } = useAdAccounts(projectId);
  
  const [activeTab, setActiveTab] = useState<'tasks' | 'timeline' | 'time' | 'ads' | 'team'>('tasks');
  const [isTimeModalOpen, setIsTimeModalOpen] = useState(false);
  const [isAdModalOpen, setIsAdModalOpen] = useState(false);
  const [isLinkAccountModalOpen, setIsLinkAccountModalOpen] = useState(false);
  
  const [newTime, setNewTime] = useState({ 
    hours: 1, 
    description: '', 
    taskId: '',
    date: new Date().toISOString().split('T')[0] 
  });
  const [newAd, setNewAd] = useState({ platform: AdPlatform.Google, spend: 0, impressions: 0, clicks: 0, conversions: 0, date: new Date().toISOString().split('T')[0] });
  const [newAccount, setNewAccount] = useState({ platform: AdPlatform.Google, externalAccountId: '' });

  const project = projects.find(p => p.id === projectId);

  if (!project) return <Container><Section>Project not found</Section></Container>;

  const handleLogTime = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newTime.taskId) {
        toast.error('Please select a task');
        return;
    }
    try {
      await logTime({ 
        ...newTime, 
        projectId, 
        date: new Date(newTime.date).toISOString() 
      });
      setIsTimeModalOpen(false);
      toast.success('Time logged successfully');
    } catch {
      toast.error('Failed to log time');
    }
  };

  const handleLogAdMetric = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await createMetric({ ...newAd, projectId, date: new Date(newAd.date).toISOString() });
      setIsAdModalOpen(false);
      toast.success('Ad metrics saved');
    } catch {
      toast.error('Failed to save metrics');
    }
  };

  const handleLinkAccount = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await linkAccount(newAccount);
      setIsLinkAccountModalOpen(false);
      toast.success('Ad account linked');
    } catch {
      toast.error('Failed to link account');
    }
  };

  const handleSync = async () => {
    try {
      await sync();
      toast.success('Sync triggered');
    } catch {
      toast.error('Sync failed');
    }
  };

  return (
    <Container>
      <Section className="border-b pb-8 flex justify-between items-end">
        <div>
            <h1 className="text-3xl font-bold">{project.name}</h1>
            <p className="text-muted-foreground mt-2 max-w-2xl">{project.description}</p>
        </div>
        {projectTeam && (
            <div className="flex gap-8 text-right">
                <div>
                    <p className="text-xs text-muted-foreground uppercase font-bold">Total Hours</p>
                    <p className="text-2xl font-mono">{projectTeam.totalHours}h</p>
                </div>
                <div>
                    <p className="text-xs text-muted-foreground uppercase font-bold">Labor Cost</p>
                    <p className="text-2xl font-mono text-rose-600">${projectTeam.estimatedLaborCost.toLocaleString()}</p>
                </div>
            </div>
        )}
      </Section>

      <div className="flex space-x-4 mt-8 bg-muted/30 p-1 rounded-lg w-fit">
        {(['tasks', 'timeline', 'time', 'ads', 'team'] as const).map(tab => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`px-4 py-2 rounded-md text-sm font-medium capitalize transition ${activeTab === tab ? 'bg-white shadow-sm text-blue-600' : 'text-muted-foreground hover:bg-white/50'}`}
          >
            {tab === 'timeline' ? 'Visual Timeline' : tab}
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

        {activeTab === 'timeline' && (
          <Section>
            <h2 className="text-xl font-semibold mb-6 text-indigo-700">Project Strategy & Timeline</h2>
            <GanttChart 
              tasks={tasks
                .filter(t => t.projectId === projectId)
                .map(t => ({
                  id: t.id,
                  title: t.title,
                  startDate: new Date(t.startDate || t.createdAt),
                  endDate: new Date(t.dueDate || t.startDate || t.createdAt),
                  status: t.status,
                  progress: t.status === 'Completed' ? 100 : t.status === 'InProgress' ? 50 : 0
                }))
              } 
            />
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
                    <TableHead>Task</TableHead>
                    <TableHead>Description</TableHead>
                    <TableHead className="text-right">Hours</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {timeEntries.map(entry => (
                    <TableRow key={entry.id}>
                      <TableCell>{new Date(entry.date).toLocaleDateString()}</TableCell>
                      <TableCell className="font-medium">{entry.taskTitle}</TableCell>
                      <TableCell>{entry.description}</TableCell>
                      <TableCell className="text-right font-bold">{entry.hours}h</TableCell>
                    </TableRow>
                  ))}
                  {timeEntries.length === 0 && <TableRow><TableCell colSpan={4} className="text-center py-8 text-muted-foreground">No hours logged yet.</TableCell></TableRow>}
                </TableBody>
            </Table>
          </Section>
        )}

        {activeTab === 'team' && (
          <Section>
             <h2 className="text-xl font-semibold mb-4 text-slate-800">Project Team</h2>
             <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Member</TableHead>
                    <TableHead>Role</TableHead>
                    <TableHead className="text-right">Rate</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {projectTeam?.members.map(member => (
                    <TableRow key={member.userId}>
                      <TableCell>
                        <div className="flex items-center gap-2">
                            <div className="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center text-blue-700 font-bold text-xs">
                                {member.userName.substring(0, 2).toUpperCase()}
                            </div>
                            <div>
                                <p className="font-medium text-sm">{member.userName}</p>
                                <p className="text-xs text-muted-foreground">{member.email}</p>
                            </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <span className="inline-flex items-center rounded-full bg-slate-100 px-2.5 py-0.5 text-xs font-medium text-slate-800">
                            {member.role}
                        </span>
                      </TableCell>
                      <TableCell className="text-right font-mono">${member.hourlyRate}/hr</TableCell>
                    </TableRow>
                  ))}
                  {(!projectTeam || projectTeam.members.length === 0) && (
                    <TableRow>
                        <TableCell colSpan={3} className="text-center py-8 text-muted-foreground">No team members assigned.</TableCell>
                    </TableRow>
                  )}
                </TableBody>
             </Table>
          </Section>
        )}

        {activeTab === 'ads' && (
          <Section>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
                <div className="bg-rose-50 p-6 rounded-xl border border-rose-100">
                    <p className="text-xs font-bold text-rose-600 uppercase">Monthly Spend</p>
                    <p className="text-3xl font-mono mt-2">${projectAnalytics?.totalSpend.toLocaleString() ?? '0'}</p>
                </div>
                <div className="bg-emerald-50 p-6 rounded-xl border border-emerald-100">
                    <p className="text-xs font-bold text-emerald-600 uppercase">Conversions</p>
                    <p className="text-3xl font-mono mt-2">{projectAnalytics?.totalConversions.toLocaleString() ?? '0'}</p>
                </div>
                <div className="bg-blue-50 p-6 rounded-xl border border-blue-100">
                    <p className="text-xs font-bold text-blue-600 uppercase">ROAS</p>
                    <p className="text-3xl font-mono mt-2">{projectAnalytics?.roas.toFixed(2) ?? '0.00'}x</p>
                </div>
            </div>

            <div className="flex justify-between items-center mb-4">
               <h2 className="text-xl font-semibold text-rose-700">Ad Accounts</h2>
               <div className="flex gap-2">
                 <Button variant="outline" size="sm" onClick={handleSync} isLoading={isSyncing}>Sync Metrics</Button>
                 <Button size="sm" onClick={() => setIsLinkAccountModalOpen(true)}>Link Account</Button>
               </div>
            </div>
            
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 mb-8">
                {accounts.map(account => (
                    <div key={account.id} className="p-4 border rounded-lg bg-white shadow-sm flex justify-between items-center">
                        <div>
                            <p className="font-bold flex items-center gap-2">
                                {account.platform === AdPlatform.Google ? 'Google Ads' : 'Meta Ads'}
                                <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
                            </p>
                            <p className="text-xs text-muted-foreground">ID: {account.externalAccountId}</p>
                        </div>
                        <Button variant="ghost" size="sm" className="text-rose-600 hover:text-rose-700" onClick={() => unlinkAccount(account.id)}>Unlink</Button>
                    </div>
                ))}
            </div>

            <div className="flex justify-between items-center mb-4 mt-12">
               <h2 className="text-xl font-semibold text-rose-700">Raw Performance Data</h2>
               <Button variant="outline" size="sm" onClick={() => setIsAdModalOpen(true)}>Manual Injection</Button>
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
                      <TableCell className="font-medium">
                        <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${m.platform === AdPlatform.Google ? 'bg-blue-100 text-blue-800' : 'bg-rose-100 text-rose-800'}`}>
                            {AdPlatform[m.platform]}
                        </span>
                      </TableCell>
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
            <div className="space-y-2">
                <label className="text-sm font-medium">Select Task</label>
                <select 
                    className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                    value={newTime.taskId}
                    onChange={e => setNewTime({ ...newTime, taskId: e.target.value })}
                    required
                >
                    <option value="">-- Choose a Task --</option>
                    {tasks.filter(t => t.projectId === projectId).map(t => (
                        <option key={t.id} value={t.id}>{t.title}</option>
                    ))}
                </select>
            </div>
            <Input label="Hours" type="number" step="0.5" value={newTime.hours} onChange={e => setNewTime({ ...newTime, hours: Number(e.target.value) })} required />
            <Input label="Date" type="date" value={newTime.date} onChange={e => setNewTime({ ...newTime, date: e.target.value })} required />
            <Input label="Description" placeholder="Working on bug fixes..." value={newTime.description} onChange={e => setNewTime({ ...newTime, description: e.target.value })} required />
            <Button type="submit" className="w-full" isLoading={isLoggingTime}>Save Entry</Button>
         </form>
      </Modal>

      <Modal isOpen={isAdModalOpen} onClose={() => setIsAdModalOpen(false)} title="Manual Metrics Injection">
         <form onSubmit={handleLogAdMetric} className="space-y-4">
            <div className="space-y-2">
                <label className="text-sm font-medium">Select Platform</label>
                <select 
                    className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                    value={newAd.platform}
                    onChange={e => setNewAd({ ...newAd, platform: Number(e.target.value) as AdPlatform })}
                    required
                >
                    <option value={AdPlatform.Google}>Google Ads</option>
                    <option value={AdPlatform.Meta}>Meta Ads</option>
                </select>
            </div>
            <Input label="Date" type="date" value={newAd.date} onChange={e => setNewAd({ ...newAd, date: e.target.value })} required />
            <Input label="Spend ($)" type="number" value={newAd.spend} onChange={e => setNewAd({ ...newAd, spend: Number(e.target.value) })} required />
            <Input label="Impressions" type="number" value={newAd.impressions} onChange={e => setNewAd({ ...newAd, impressions: Number(e.target.value) })} required />
            <Input label="Conversions" type="number" value={newAd.conversions} onChange={e => setNewAd({ ...newAd, conversions: Number(e.target.value) })} required />
            <Button type="submit" className="w-full" isLoading={isLoggingMetric}>Save Metrics</Button>
         </form>
      </Modal>

      {/* Link Ad Account Modal */}
      <Modal isOpen={isLinkAccountModalOpen} onClose={() => setIsLinkAccountModalOpen(false)} title="Link External Ad Account">
         <form onSubmit={handleLinkAccount} className="space-y-4">
            <div className="space-y-2">
                <label className="text-sm font-medium">Select Platform</label>
                <select 
                    className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                    value={newAccount.platform}
                    onChange={e => setNewAccount({ ...newAccount, platform: Number(e.target.value) as AdPlatform })}
                    required
                >
                    <option value={AdPlatform.Google}>Google Ads</option>
                    <option value={AdPlatform.Meta}>Meta Ads</option>
                </select>
            </div>
            <Input label="Account ID" placeholder="e.g. act_123456789 or 123-456-7890" value={newAccount.externalAccountId} onChange={e => setNewAccount({ ...newAccount, externalAccountId: e.target.value })} required />
            <p className="text-xs text-muted-foreground bg-blue-50 p-3 rounded border border-blue-100 italic">
                Notice: For the MVP, we use stable stubs to simulate daily platform sync. Real OAuth2 flow will be enabled in Phase 4.
            </p>
            <Button type="submit" className="w-full" isLoading={isLinking}>Link & Initial Sync</Button>
         </form>
      </Modal>
    </Container>
  );
}
