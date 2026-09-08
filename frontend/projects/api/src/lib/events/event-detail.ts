import { EventInput } from './event-input';
import { EventSummary } from './event-summary';
import { LogoMetadata } from './logo-metadata';

export interface EventDetail extends EventSummary, EventInput { logo: LogoMetadata | null; }
