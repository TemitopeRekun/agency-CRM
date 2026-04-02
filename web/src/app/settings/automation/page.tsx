'use client';

import { useState } from 'react';
import { useAutomation } from '@/hooks/queries/useAutomation';
import { 
  Plus, 
  Trash2, 
  Zap, 
  Settings, 
  AlertCircle, 
  Info,
  CheckCircle2,
  Clock
} from 'lucide-react';

export default function AutomationSettingsPage() {
  const { 
    templates, 
    isLoading, 
    createTemplate, 
    deleteTemplate, 
    triggerOverdueCheck,
    isTriggering 
  } = useAutomation();

  const [newTemplate, setNewTemplate] = useState({
    serviceType: '',
    taskTitle: '',
    taskDescription: '',
    defaultPriority: 'Normal',
  });

  const handleCreate = async () => {
    if (!newTemplate.serviceType || !newTemplate.taskTitle) return;
    await createTemplate(newTemplate);
    setNewTemplate({
      serviceType: '',
      taskTitle: '',
      taskDescription: '',
      defaultPriority: 'Normal',
    });
  };

  return (
    <div className="p-8 max-w-6xl mx-auto space-y-8 animate-in fade-in duration-500">
      <header className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
            <Zap className="h-8 w-8 text-indigo-600" />
            Automation Engine
          </h1>
          <p className="text-gray-500 mt-2">
            Configure how the system handles events and triggers tasks automatically.
          </p>
        </div>
        <div className="flex gap-4">
          <button
            onClick={() => triggerOverdueCheck()}
            disabled={isTriggering}
            className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-200 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors disabled:opacity-50"
          >
            {isTriggering ? <Clock className="h-4 w-4 animate-spin" /> : <AlertCircle className="h-4 w-4" />}
            Trigger Overdue Check
          </button>
        </div>
      </header>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Task Templates Section */}
        <div className="lg:col-span-2 space-y-6">
          <div className="bg-white rounded-2xl border border-gray-200 shadow-sm overflow-hidden">
            <div className="p-6 border-b border-gray-100 flex justify-between items-center bg-gray-50/50">
              <h2 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                <Settings className="h-5 w-5 text-indigo-500" />
                Service-to-Task Mapping
              </h2>
            </div>
            
            <div className="p-6 space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1">
                  <label className="text-sm font-medium text-gray-700">Service Keyword</label>
                  <input
                    type="text"
                    placeholder="e.g. Google Ads, SEO"
                    className="w-full px-4 py-2 rounded-lg border border-gray-200 focus:ring-2 focus:ring-indigo-500 outline-none"
                    value={newTemplate.serviceType}
                    onChange={(e) => setNewTemplate({ ...newTemplate, serviceType: e.target.value })}
                  />
                </div>
                <div className="space-y-1">
                  <label className="text-sm font-medium text-gray-700">Priority</label>
                  <select
                    className="w-full px-4 py-2 rounded-lg border border-gray-200 focus:ring-2 focus:ring-indigo-500 outline-none bg-white"
                    value={newTemplate.defaultPriority}
                    onChange={(e) => setNewTemplate({ ...newTemplate, defaultPriority: e.target.value })}
                  >
                    <option value="Low">Low</option>
                    <option value="Normal">Normal</option>
                    <option value="High">High</option>
                    <option value="Urgent">Urgent</option>
                  </select>
                </div>
              </div>
              <div className="space-y-1">
                <label className="text-sm font-medium text-gray-700">Task Title</label>
                <input
                  type="text"
                  placeholder="The task that will be created"
                  className="w-full px-4 py-2 rounded-lg border border-gray-200 focus:ring-2 focus:ring-indigo-500 outline-none"
                  value={newTemplate.taskTitle}
                  onChange={(e) => setNewTemplate({ ...newTemplate, taskTitle: e.target.value })}
                />
              </div>
              <div className="space-y-1">
                <label className="text-sm font-medium text-gray-700">Task Description</label>
                <textarea
                  placeholder="Describe the SOP for this task..."
                  rows={3}
                  className="w-full px-4 py-2 rounded-lg border border-gray-200 focus:ring-2 focus:ring-indigo-500 outline-none resize-none"
                  value={newTemplate.taskDescription}
                  onChange={(e) => setNewTemplate({ ...newTemplate, taskDescription: e.target.value })}
                />
              </div>
              <button
                onClick={handleCreate}
                disabled={!newTemplate.serviceType || !newTemplate.taskTitle}
                className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors disabled:opacity-50 font-medium"
              >
                <Plus className="h-4 w-4" />
                Add Mapping
              </button>
            </div>
          </div>

          <div className="space-y-4">
            {isLoading ? (
              <div className="p-8 text-center text-gray-500">Loading templates...</div>
            ) : templates.length === 0 ? (
              <div className="p-12 text-center bg-gray-50 rounded-2xl border-2 border-dashed border-gray-200">
                <Info className="h-8 w-8 text-gray-400 mx-auto mb-3" />
                <p className="text-gray-500 font-medium">No automation rules configured yet.</p>
                <p className="text-sm text-gray-400 mt-1">Add your first service-to-task mapping above.</p>
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {templates.map((template) => (
                  <div key={template.id} className="bg-white p-5 rounded-xl border border-gray-200 shadow-sm hover:shadow-md transition-shadow relative group">
                    <button
                      onClick={() => deleteTemplate(template.id)}
                      className="absolute top-4 right-4 p-2 text-gray-400 hover:text-red-500 opacity-0 group-hover:opacity-100 transition-all"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                    <div className="flex items-center gap-2 mb-3">
                      <span className="px-2 py-1 bg-indigo-50 text-indigo-700 text-xs font-bold rounded uppercase tracking-wider">
                        {template.serviceType}
                      </span>
                      <span className={`px-2 py-1 text-xs font-medium rounded ${
                        template.defaultPriority === 'High' || template.defaultPriority === 'Urgent' 
                          ? 'bg-red-50 text-red-700' 
                          : 'bg-green-50 text-green-700'
                      }`}>
                        {template.defaultPriority}
                      </span>
                    </div>
                    <h3 className="font-semibold text-gray-900 mb-1">{template.taskTitle}</h3>
                    <p className="text-sm text-gray-500 line-clamp-2">{template.taskDescription}</p>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Sidebar Status/Info */}
        <div className="space-y-6">
          <div className="bg-indigo-900 text-white p-6 rounded-2xl shadow-xl">
            <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
              <CheckCircle2 className="h-5 w-5" />
              Active Automations
            </h3>
            <ul className="space-y-4 text-sm text-indigo-100">
              <li className="flex gap-3">
                <div className="h-5 w-5 bg-indigo-800 rounded flex items-center justify-center shrink-0">1</div>
                <p><span className="text-white font-medium">Offer Acceptance:</span> Auto-creates Project, Contract (Draft), and Tasks based on line items.</p>
              </li>
              <li className="flex gap-3">
                <div className="h-5 w-5 bg-indigo-800 rounded flex items-center justify-center shrink-0">2</div>
                <p><span className="text-white font-medium">Overdue Check:</span> Daily scan for overdue invoices with internal system alerts.</p>
              </li>
              <li className="flex gap-3">
                <div className="h-5 w-5 bg-indigo-800 rounded flex items-center justify-center shrink-0">3</div>
                <p><span className="text-white font-medium">Ad Metrics:</span> Daily batch sync from Google/Meta/TikTok stubs.</p>
              </li>
            </ul>
          </div>

          <div className="bg-amber-50 border border-amber-200 p-6 rounded-2xl">
            <h3 className="text-amber-900 font-semibold mb-2 flex items-center gap-2">
              <Info className="h-5 w-5" />
              How it works
            </h3>
            <p className="text-sm text-amber-800 leading-relaxed">
              The system uses &quot;Modular Mapping&quot;. When an offer is accepted, it scans the Title and Notes for your 
              <strong>Service Keywords</strong>. If a match is found, all related tasks are automatically added 
              to the new project workflow.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
