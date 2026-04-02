'use client';

import { Container, Section, Card, CardHeader, CardTitle, CardContent } from '@/components/ui/LayoutPrimitives';
import { ROIAnalytics } from '../dashboard/components/ROIAnalytics';
import { useAdMetrics } from '@/hooks/queries/useAdMetrics';
import { useProjects } from '@/hooks/queries/useProjects';

export default function AnalyticsPage() {
  const { analytics, isAnalyticsLoading } = useAdMetrics();
  const { projects } = useProjects();

  return (
    <Container>
      <Section className="border-b pb-8">
        <h1 className="text-3xl font-bold">Agency Performance Intelligence</h1>
        <p className="text-muted-foreground mt-2">Real-time ROI and Advertising Metrics across all managed accounts.</p>
      </Section>

      <div className="mt-8">
        <ROIAnalytics />
      </div>

      <Section title="Project Profitability Breakdown" className="mt-12">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {projects.map(project => (
            <Card key={project.id} className="border-l-4 border-l-blue-500">
              <CardHeader>
                <CardTitle className="text-lg">{project.name}</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="flex justify-between items-end">
                   <div>
                      <p className="text-xs text-muted-foreground uppercase font-bold">Status</p>
                      <p className="text-sm font-medium mt-1">{project.status}</p>
                   </div>
                   <div className="text-right">
                      <a 
                        href={`/projects/${project.id}`} 
                        className="text-xs text-blue-600 font-bold hover:underline"
                      >
                        View ROI Details →
                      </a>
                   </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      </Section>

      <Section title="Intelligence Insights" className="mt-12">
         <div className="bg-slate-900 rounded-2xl p-8 text-white">
            <h3 className="text-xl font-bold mb-4 flex items-center gap-2">
                <span className="text-emerald-400">●</span> AI Business Insights (Alpha)
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                <div className="space-y-4">
                    <p className="text-slate-300 text-sm leading-relaxed">
                        Based on current trends, your average agency ROAS is <span className="text-emerald-400 font-bold">{analytics?.roas.toFixed(2)}x</span>. 
                        We recommend increasing budget for projects with ROAS > 4.0 to maximize total revenue share.
                    </p>
                    <div className="p-4 bg-white/5 rounded-lg border border-white/10">
                        <p className="text-xs font-bold text-slate-500 uppercase">Top Performer</p>
                        <p className="text-sm mt-1">Google Ads is currently driving 65% of total conversions.</p>
                    </div>
                </div>
                <div className="space-y-4 font-mono text-xs text-slate-400">
                    <div className="flex justify-between border-b border-white/10 pb-2">
                        <span>Total Managed Spend:</span>
                        <span className="text-white">${analytics?.totalSpend.toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between border-b border-white/10 pb-2">
                        <span>Total Impressions:</span>
                        <span className="text-white">{analytics?.totalImpressions.toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between border-b border-white/10 pb-2">
                        <span>Avg. CPC:</span>
                        <span className="text-white">${(analytics?.totalSpend ?? 0) / (analytics?.totalClicks ?? 1) > 0 ? ((analytics?.totalSpend ?? 0) / (analytics?.totalClicks ?? 1)).toFixed(2) : '0.00'}</span>
                    </div>
                </div>
            </div>
         </div>
      </Section>
    </Container>
  );
}
