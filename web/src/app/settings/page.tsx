'use client';

import { Container, Section } from '@/components/ui/LayoutPrimitives';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { useState } from 'react';
import { toast } from 'sonner';
import Link from 'next/link';
import { ArrowLeft, Building2, CreditCard, Zap, Link as LinkIcon } from 'lucide-react';

export default function SettingsPage() {
  const [isSaving, setIsSaving] = useState(false);
  const [settings, setSettings] = useState({
    agencyName: 'My Digital Agency',
    taxId: 'VAT-12345678',
    address: '123 Agency Way, London',
    email: 'billing@agency.com',
    defaultCurrency: 'USD',
    autoInvoice: true
  });

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    // Simulate API call
    await new Promise(r => setTimeout(r, 800));
    setIsSaving(false);
    toast.success('Settings updated successfully');
  };

  return (
    <Container className="pb-20">
      <Section className="flex items-center gap-4 mb-8">
        <Link href="/dashboard">
          <Button variant="ghost" size="sm" className="rounded-full h-8 w-8 p-0">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Settings</h1>
          <p className="text-muted-foreground">Manage your agency profile and global preferences.</p>
        </div>
      </Section>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
        {/* Navigation Sidebar */}
        <div className="space-y-2">
          <Button variant="secondary" className="w-full justify-start gap-2 bg-slate-100 border-l-4 border-l-blue-600">
            <Building2 className="h-4 w-4" /> Agency Profile
          </Button>
          <Link href="/settings/automation">
            <Button variant="ghost" className="w-full justify-start gap-2">
              <Zap className="h-4 w-4" /> Automation Engine
            </Button>
          </Link>
          <Link href="/integrations">
            <Button variant="ghost" className="w-full justify-start gap-2">
              <LinkIcon className="h-4 w-4" /> Channel Integrations
            </Button>
          </Link>
        </div>

        {/* Content Area */}
        <div className="md:col-span-2 space-y-8">
          <form onSubmit={handleSave} className="space-y-6">
            {/* Agency Profile */}
            <Section className="bg-white p-6 rounded-xl border shadow-sm">
              <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
                <Building2 className="h-5 w-5 text-blue-600" />
                Business Identity
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Input 
                  label="Legal Agency Name" 
                  value={settings.agencyName} 
                  onChange={(e) => setSettings({...settings, agencyName: e.target.value})}
                />
                <Input 
                  label="VAT/Tax ID" 
                  value={settings.taxId} 
                  onChange={(e) => setSettings({...settings, taxId: e.target.value})}
                />
                <div className="md:col-span-2">
                  <Input 
                    label="Business Address" 
                    value={settings.address} 
                    onChange={(e) => setSettings({...settings, address: e.target.value})}
                  />
                </div>
              </div>
            </Section>

            {/* Billing & Finance */}
            <Section className="bg-white p-6 rounded-xl border shadow-sm">
              <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
                <CreditCard className="h-5 w-5 text-emerald-600" />
                Billing & Finance
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <label className="text-sm font-medium">Global Currency</label>
                  <select 
                    className="w-full border rounded px-3 py-2 bg-background"
                    value={settings.defaultCurrency}
                    onChange={(e) => setSettings({...settings, defaultCurrency: e.target.value})}
                  >
                    <option value="USD">USD ($)</option>
                    <option value="EUR">EUR (€)</option>
                    <option value="GBP">GBP (£)</option>
                  </select>
                </div>
                <div className="flex items-center gap-3 mt-8">
                    <input 
                      type="checkbox" 
                      id="autoInvoice" 
                      className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-600"
                      checked={settings.autoInvoice}
                      onChange={(e) => setSettings({...settings, autoInvoice: e.target.checked})}
                    />
                    <label htmlFor="autoInvoice" className="text-sm font-medium leading-none">
                      Enable Automated Monthly Billing
                    </label>
                </div>
              </div>
              <p className="text-xs text-muted-foreground mt-4 italic">
                * Automated billing triggers draft invoice generation on the contract anniversary date.
              </p>
            </Section>

            <div className="flex justify-end pt-4">
              <Button type="submit" isLoading={isSaving} className="px-8 bg-blue-600 hover:bg-blue-700">
                Save All Changes
              </Button>
            </div>
          </form>
        </div>
      </div>
    </Container>
  );
}
