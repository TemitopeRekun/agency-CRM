'use client';

import { useAdMetrics } from '@/hooks/queries/useAdMetrics';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/LayoutPrimitives';
import { 
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend,
  BarChart, Bar
} from 'recharts';

interface ROIAnalyticsProps {
  projectId?: string;
}

export const ROIAnalytics = ({ projectId }: ROIAnalyticsProps) => {
  const { metrics, analytics, isAnalyticsLoading } = useAdMetrics(projectId);

  if (isAnalyticsLoading) {
    return <div className="h-[300px] flex items-center justify-center bg-muted animate-pulse rounded-lg">Loading ROI Analytics...</div>;
  }

  const chartData = metrics.map(m => ({
    date: new Date(m.date).toLocaleDateString(),
    spend: m.spend,
    leads: m.conversions,
    clicks: m.clicks
  }));

  return (
    <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
      {/* Primary ROI Card */}
      <Card className="lg:col-span-1 bg-slate-900 text-white">
        <CardHeader>
          <CardTitle className="text-sm font-medium opacity-70">Project ROI</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-4xl font-bold">
             {analytics?.projectROI.toFixed(1)}%
          </div>
          <p className="text-xs text-emerald-400 mt-2 font-medium">
             ROAS: {analytics?.roas.toFixed(2)}x
          </p>
          <div className="mt-6 space-y-2">
             <div className="flex justify-between text-xs font-semibold uppercase opacity-60">
                <span>Total Spend</span>
                <span>${analytics?.totalSpend.toLocaleString()}</span>
             </div>
             <div className="w-full bg-white/10 h-1.5 rounded-full overflow-hidden">
                <div className="bg-emerald-500 h-full" style={{ width: '70%' }} />
             </div>
          </div>
        </CardContent>
      </Card>

      {/* CPL Card */}
      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-medium text-muted-foreground uppercase">Cost Per Lead (CPL)</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-4xl font-bold text-rose-600">
             ${analytics?.costPerLead.toFixed(2)}
          </div>
          <p className="text-xs text-muted-foreground mt-2">
             Target: &lt;$50.00
          </p>
          <div className="h-[60px] mt-4">
             <ResponsiveContainer width="100%" height="100%">
                <BarChart data={chartData.slice(-5)}>
                   <Bar dataKey="spend" fill="#f43f5e" radius={[2, 2, 0, 0]} />
                </BarChart>
             </ResponsiveContainer>
          </div>
        </CardContent>
      </Card>

      {/* Performance Distribution */}
      <Card className="lg:col-span-1">
        <CardHeader>
          <CardTitle className="text-sm font-medium text-muted-foreground uppercase">Conv. Rate</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-3xl font-bold text-blue-600">
             {analytics?.conversionRate.toFixed(1)}%
          </div>
          <p className="text-xs text-muted-foreground mt-2">
             CTR: {analytics?.ctr.toFixed(1)}%
          </p>
          <div className="mt-4 flex flex-col gap-2">
              <div className="flex justify-between text-xs">
                 <span>Impressions</span>
                 <span className="font-bold">{analytics?.totalImpressions.toLocaleString()}</span>
              </div>
              <div className="flex justify-between text-xs">
                 <span>Total Clicks</span>
                 <span className="font-bold">{analytics?.totalClicks.toLocaleString()}</span>
              </div>
          </div>
        </CardContent>
      </Card>

      {/* Main Trends Chart */}
      <Card className="md:col-span-2 lg:col-span-3">
        <CardHeader>
          <CardTitle>Ad Performance Trends</CardTitle>
        </CardHeader>
        <CardContent className="h-[300px]">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} />
              <XAxis dataKey="date" fontSize={12} tickLine={false} axisLine={false} />
              <YAxis fontSize={12} tickLine={false} axisLine={false} />
              <Tooltip />
              <Legend verticalAlign="top" align="right" height={36} iconType="circle" />
              <Line type="monotone" dataKey="spend" stroke="#f43f5e" strokeWidth={2} dot={false} name="Spend ($)" />
              <Line type="monotone" dataKey="leads" stroke="#10b981" strokeWidth={2} dot={false} name="Leads" />
              <Line type="monotone" dataKey="clicks" stroke="#3b82f6" strokeWidth={2} dot={false} name="Clicks" />
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>
    </div>
  );
};
