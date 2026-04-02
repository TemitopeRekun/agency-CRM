'use client';

import { Container, Section, Card, CardHeader, CardTitle, CardContent } from '@/components/ui/LayoutPrimitives';
import { Button } from '@/components/ui/Button';
import { toast } from 'sonner';

export default function IntegrationsPage() {
  const webhookBaseUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8000';

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
    toast.success('Webhook URL copied to clipboard');
  };

  const integrations = [
    {
      name: 'Meta Ads (Facebook/Instagram)',
      description: 'Ingest leads directly from Meta Lead Ads in real-time.',
      webhookUrl: `${webhookBaseUrl}/api/webhooks/meta/lead`,
      status: 'Ready',
      iconColor: 'bg-blue-600'
    },
    {
      name: 'Google Ads',
      description: 'Track spend and performance metrics natively.',
      webhookUrl: `${webhookBaseUrl}/api/webhooks/google/performance`,
      status: 'Ready',
      iconColor: 'bg-red-500'
    },
    {
      name: 'TikTok Ads',
      description: 'Automated performance reporting for TikTok campaigns.',
      webhookUrl: `${webhookBaseUrl}/api/webhooks/tiktok/performance`,
      status: 'Ready',
      iconColor: 'bg-black'
    }
  ];

  return (
    <Container>
      <Section className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Integrations & Webhooks</h1>
      </Section>

      <Section>
        <div className="grid gap-6">
          {integrations.map((app) => (
            <Card key={app.name} className="overflow-hidden">
               <div className="flex flex-col md:flex-row">
                  <div className={`w-full md:w-2 ${app.iconColor}`} />
                  <div className="flex-1 p-6">
                    <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                      <div>
                        <CardTitle className="text-xl">{app.name}</CardTitle>
                        <p className="text-sm text-muted-foreground mt-1">{app.description}</p>
                      </div>
                      <div className="flex items-center gap-3">
                         <span className="text-xs font-semibold px-2 py-1 bg-green-100 text-green-800 rounded-full">
                            {app.status}
                         </span>
                         <Button variant="outline" size="sm">Configure</Button>
                      </div>
                    </div>

                    <div className="mt-6 p-4 bg-slate-50 rounded-lg border border-slate-200">
                       <label className="text-xs font-bold uppercase text-slate-500 block mb-2">Native Webhook URL</label>
                       <div className="flex items-center gap-2">
                          <code className="flex-1 text-xs bg-white p-2 rounded border truncate">
                            {app.webhookUrl}
                          </code>
                          <Button 
                            variant="secondary" 
                            size="sm"
                            onClick={() => copyToClipboard(app.webhookUrl)}
                          >
                            Copy
                          </Button>
                       </div>
                       <p className="text-[10px] text-slate-400 mt-2">
                          Paste this URL into your {app.name.split(' ')[0]} Developer Dashboard to enable automatic ingestion.
                       </p>
                    </div>
                  </div>
               </div>
            </Card>
          ))}
        </div>
      </Section>

      <Section title="API Analytics Infrastructure" className="mt-8">
         <Card className="bg-slate-900 text-white">
            <CardContent className="py-8">
               <div className="flex flex-col md:flex-row items-center gap-8 px-4">
                  <div className="flex-1">
                     <h3 className="text-xl font-bold mb-2">Powering Agency Intelligence</h3>
                     <p className="text-slate-300 text-sm leading-relaxed">
                        These native connectors feed directly into our ROI calculation engine. 
                        By bridging the gap between ad spend and legal contracts, we provide real-time 
                        Cost Per Lead (CPL) and Return on Ad Spend (ROAS) analytics.
                     </p>
                  </div>
                  <div className="flex gap-4">
                     <div className="h-16 w-16 rounded-full bg-white/10 flex items-center justify-center font-bold text-2xl">A</div>
                     <div className="h-16 w-16 rounded-full bg-white/10 flex items-center justify-center font-bold text-2xl">I</div>
                  </div>
               </div>
            </CardContent>
         </Card>
      </Section>
    </Container>
  );
}
