'use client';

import { useState } from 'react';
import { useOffers, OfferStatus } from '@/hooks/queries/useOffers';
import { Button } from '@/components/ui/Button';
import { Container, Section } from '@/components/ui/LayoutPrimitives';
import { NewOfferModal } from './components/NewOfferModal';

const COLUMN_NAMES = {
  [OfferStatus.Draft]: 'Draft',
  [OfferStatus.Sent]: 'Sent',
  [OfferStatus.Accepted]: 'Accepted',
  [OfferStatus.Rejected]: 'Rejected'
};

export default function OffersPage() {
  const { offers, isLoading, updateStatus, isUpdatingStatus } = useOffers();
  const [isModalOpen, setIsModalOpen] = useState(false);

  const handleDragStart = (e: React.DragEvent, offerId: string) => {
    e.dataTransfer.setData('offerId', offerId);
  };

  const handleDrop = async (e: React.DragEvent, newStatus: OfferStatus) => {
    e.preventDefault();
    const offerId = e.dataTransfer.getData('offerId');
    if (offerId && !isUpdatingStatus) {
      const offer = offers.find(o => o.id === offerId);
      if (offer && offer.status !== newStatus) {
        await updateStatus({ id: offerId, status: newStatus });
      }
    }
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault(); 
  };

  const renderColumn = (status: OfferStatus) => {
    const columnOffers = offers.filter(o => o.status === status);
    
    return (
      <div 
        key={status}
        className="flex-1 min-w-[250px] bg-muted/30 rounded-lg p-4 flex flex-col gap-3 min-h-[500px]"
        onDrop={(e) => handleDrop(e, status)}
        onDragOver={handleDragOver}
      >
        <div className="flex justify-between items-center mb-2">
          <h3 className="font-semibold text-sm text-muted-foreground uppercase tracking-wider">
            {COLUMN_NAMES[status]}
          </h3>
          <span className="text-xs bg-muted px-2 py-1 rounded-full">{columnOffers.length}</span>
        </div>
        
        {columnOffers.map(offer => (
          <div 
            key={offer.id}
            draggable
            onDragStart={(e) => handleDragStart(e, offer.id)}
            className={`bg-background border rounded-lg p-4 shadow-sm cursor-grab active:cursor-grabbing hover:shadow-md transition-shadow relative ${isUpdatingStatus ? 'opacity-50 pointer-events-none' : ''}`}
          >
            {offer.quoteTemplateId && (
              <span className="text-xs bg-indigo-100 text-indigo-800 px-2 py-0.5 rounded-full inline-block mb-2 capitalize">
                {offer.quoteTemplateId.replace('template-', '')}
              </span>
            )}
            <h4 className="font-medium mb-1">{offer.title}</h4>
            <div className="text-xl font-bold text-emerald-600 mb-3">
               ${offer.totalAmount?.toLocaleString() || '0'}
            </div>
            {offer.hasBeenViewed ? (
              <div className="text-xs text-blue-600 font-medium mb-2 flex items-center gap-1">
                 <span className="w-2 h-2 rounded-full bg-blue-600"></span> Client Viewed
              </div>
            ) : (
                <div className="text-xs text-amber-600 font-medium mb-2 flex items-center gap-1">
                 <span className="w-2 h-2 rounded-full bg-amber-600"></span> Not Viewed
              </div>
            )}
            {offer.notes && <p className="text-xs text-muted-foreground line-clamp-2">{offer.notes}</p>}
          </div>
        ))}
        {columnOffers.length === 0 && (
          <div className="text-center text-sm text-muted-foreground py-8 border-2 border-dashed rounded-lg border-muted">
            Drop here
          </div>
        )}
      </div>
    );
  };

  return (
    <Container>
      <Section className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Offers Pipeline</h1>
        <Button onClick={() => setIsModalOpen(true)}>New Offer</Button>
      </Section>

      <Section>
        {isLoading ? (
          <div className="animate-pulse flex gap-4">
            {[1, 2, 3, 4].map(i => <div key={i} className="flex-1 min-w-[250px] bg-muted h-[500px] rounded-lg" />)}
          </div>
        ) : (
          <div className="flex gap-4 overflow-x-auto pb-4">
            {renderColumn(OfferStatus.Draft)}
            {renderColumn(OfferStatus.Sent)}
            {renderColumn(OfferStatus.Accepted)}
            {renderColumn(OfferStatus.Rejected)}
          </div>
        )}
      </Section>

      <NewOfferModal 
        isOpen={isModalOpen} 
        onClose={() => setIsModalOpen(false)} 
      />
    </Container>
  );
}
