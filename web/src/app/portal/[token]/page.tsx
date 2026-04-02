'use client';

import React, { useState, useEffect } from 'react';
import { useParams } from 'next/navigation';
import { useContractPortal } from '@/hooks/queries/useContractPortal';
import { SignaturePad } from '@/components/portal/SignaturePad';
import { 
  FileText, 
  ShieldCheck, 
  Calendar, 
  DollarSign, 
  CheckCircle2, 
  AlertTriangle,
  Loader2,
  ExternalLink
} from 'lucide-react';

export default function ContractPortalPage() {
  const params = useParams();
  const token = params.token as string;
  const { contract, isLoading, error, sign, isSigning, markViewed } = useContractPortal(token);
  const [signedSuccess, setSignedSuccess] = useState(false);

  useEffect(() => {
    if (token && markViewed) {
      markViewed().catch(console.error);
    }
  }, [token, markViewed]);

  const handleSign = async (dataUrl: string) => {
    try {
      await sign(dataUrl);
      setSignedSuccess(true);
    } catch (err) {
      console.error('Signing failed:', err);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <Loader2 className="h-8 w-8 text-indigo-600 animate-spin" />
      </div>
    );
  }

  if (error || !contract) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4">
        <div className="max-w-md w-full bg-white p-8 rounded-2xl shadow-sm border border-gray-200 text-center">
          <AlertTriangle className="h-12 w-12 text-red-500 mx-auto mb-4" />
          <h1 className="text-xl font-bold text-gray-900">Invalid or Expired Link</h1>
          <p className="text-gray-500 mt-2">
            This contract link is no longer valid. Please contact your account manager for a new one.
          </p>
        </div>
      </div>
    );
  }

  if (signedSuccess || contract.status === 'Signed') {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4 animate-in fade-in duration-700">
        <div className="max-w-md w-full bg-white p-12 rounded-3xl shadow-xl border border-gray-100 text-center">
          <div className="h-20 w-20 bg-green-100 text-green-600 rounded-full flex items-center justify-center mx-auto mb-6 scale-in duration-500 transition-all">
            <CheckCircle2 className="h-10 w-10" />
          </div>
          <h1 className="text-3xl font-bold text-gray-900">Contract Signed!</h1>
          <p className="text-gray-500 mt-4 leading-relaxed">
            Thank you for choosing our agency. We've received your signature and our team has been notified. 
            A copy of the signed agreement will be sent to your email shortly.
          </p>
          <div className="mt-8 pt-8 border-t border-gray-100">
            <p className="text-sm text-gray-400 font-medium uppercase tracking-widest">Signed on</p>
            <p className="text-gray-900 font-semibold mt-1">
              {new Date(contract.signedAt || new Date()).toLocaleDateString()}
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50/50 py-12 px-4 sm:px-6 lg:px-8 animate-in fade-in slide-in-from-bottom-4 duration-700">
      <div className="max-w-4xl mx-auto">
        <header className="text-center mb-12">
          <div className="inline-flex items-center gap-2 px-3 py-1 bg-indigo-50 text-indigo-700 rounded-full text-sm font-bold uppercase tracking-wider mb-4 border border-indigo-100">
            <ShieldCheck className="h-4 w-4" />
            Secure Proposal Portal
          </div>
          <h1 className="text-4xl font-extrabold text-gray-900 tracking-tight">{contract.title}</h1>
          <p className="mt-4 text-lg text-gray-500">Review and electronically sign your service agreement below.</p>
        </header>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Main Contract View */}
          <div className="lg:col-span-2 space-y-8">
            <section className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden">
              <div className="p-6 border-b border-gray-100 bg-gray-50/50 flex items-center justify-between">
                <h2 className="text-lg font-bold text-gray-900 flex items-center gap-2">
                  <FileText className="h-5 w-5 text-indigo-500" />
                  Agreement Terms
                </h2>
                <span className="text-sm text-gray-400 font-medium">Ref: {contract.id.slice(0, 8)}</span>
              </div>
              <div className="p-8 prose prose-indigo max-w-none">
                <div className="whitespace-pre-wrap text-gray-700 leading-relaxed font-serif text-lg">
                  {contract.terms || "Standard agency terms apply..."}
                </div>
              </div>
            </section>

            <section className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden overflow-visible">
              <div className="p-6 border-b border-gray-100 bg-gray-50/50">
                <h2 className="text-lg font-bold text-gray-900 flex items-center gap-2">
                  Draw your Signature
                </h2>
              </div>
              <div className="p-8">
                <SignaturePad onSave={handleSign} isLoading={isSigning} />
                <p className="mt-4 text-xs text-gray-400 text-center leading-relaxed">
                  By signing this document, you agree to the terms listed above. 
                  This signature is legally binding as per the E-Sign Act. 
                  Your IP address and timestamp will be recorded for security purposes.
                </p>
              </div>
            </section>
          </div>

          {/* Sidebar Info */}
          <div className="space-y-6">
            <div className="bg-indigo-600 rounded-2xl p-6 text-white shadow-xl shadow-indigo-200 h-fit sticky top-8">
              <h3 className="text-indigo-100 text-sm font-bold uppercase tracking-widest mb-4">Investment Summary</h3>
              <div className="space-y-6">
                <div>
                  <p className="text-indigo-200 text-sm">Total Project Amount</p>
                  <p className="text-3xl font-bold flex items-center gap-1 mt-1">
                    <DollarSign className="h-6 w-6 text-indigo-300" />
                    {contract.totalAmount.toLocaleString()}
                  </p>
                </div>
                <div className="pt-6 border-t border-indigo-500/50 flex items-center gap-3">
                  <Calendar className="h-5 w-5 text-indigo-300" />
                  <div className="text-sm">
                    <p className="text-indigo-200">Prepared on</p>
                    <p className="font-semibold">{new Date(contract.createdAt).toLocaleDateString()}</p>
                  </div>
                </div>
                <div className="space-y-3 pt-6 border-t border-indigo-500/50">
                  <p className="text-indigo-200 text-sm">Includes:</p>
                  <ul className="text-sm space-y-2">
                    <li className="flex items-center gap-2">
                      <div className="h-1.5 w-1.5 bg-indigo-300 rounded-full" />
                      Full project execution
                    </li>
                    <li className="flex items-center gap-2">
                      <div className="h-1.5 w-1.5 bg-indigo-300 rounded-full" />
                      Monthly performance reports
                    </li>
                    <li className="flex items-center gap-2">
                      <div className="h-1.5 w-1.5 bg-indigo-300 rounded-full" />
                      Dedicated account manager
                    </li>
                  </ul>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-2xl p-6 border border-gray-200 shadow-sm">
              <h3 className="text-gray-900 font-bold mb-3 flex items-center gap-2">
                Need Help?
              </h3>
              <p className="text-sm text-gray-500 mb-4">
                Have questions about this proposal before signing?
              </p>
              <button className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-gray-50 border border-gray-200 text-gray-700 rounded-lg hover:bg-gray-100 transition-colors font-medium">
                Contact Account Manager
                <ExternalLink className="h-4 w-4" />
              </button>
            </div>
          </div>
        </div>
      </div>
      <footer className="max-w-4xl mx-auto mt-12 text-center text-gray-400 text-sm">
        &copy; {new Date().getFullYear()} Modern Digital Agency. Securely processed by Agency CRM.
      </footer>
    </div>
  );
}
