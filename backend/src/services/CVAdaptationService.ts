import OpenAI from 'openai';

interface CVData {
  name: string;
  title: string;
  location: string;
  phone: string;
  email: string;
  website: string;
  linkedin: string;
  summary: string;
  skills: Array<{
    category: string;
    items: string[];
  }>;
  experience: Array<{
    title: string;
    company: string;
    location: string;
    dates: string;
    bullets: string[];
  }>;
  projects: Array<{
    title: string;
    role: string;
    bullets: string[];
  }>;
  education: Array<{
    degree: string;
    school: string;
    location: string;
    dates: string;
    bullets: string[];
  }>;
}

interface JobOfferData {
  job_offer_raw: string;
  job_offer_origin: string;
  job_offer_company: string;
  job_offer_name: string;
}

interface AdaptCVRequest {
  cv: CVData;
  jobOffer: JobOfferData;
  sessionId: string;
}

export class CVAdaptationService {
  private openai: OpenAI;
  private conversationHistory: Map<
    string,
    OpenAI.Chat.ChatCompletionMessageParam[]
  >;

  constructor() {
    const apiKey = process.env.OPENAI_API_KEY;
    if (!apiKey) {
      throw new Error('OPENAI_API_KEY environment variable is required');
    }

    this.openai = new OpenAI({ apiKey });
    this.conversationHistory = new Map();
  }

  /**
   * Adapt CV based on job offer using OpenAI
   */
  async adaptCV(request: AdaptCVRequest): Promise<CVData> {
    const { cv, jobOffer, sessionId } = request;

    // Get or initialize conversation history for this session
    if (!this.conversationHistory.has(sessionId)) {
      this.conversationHistory.set(sessionId, []);
    }
    const history = this.conversationHistory.get(sessionId)!;

    // Build the user prompt
    const userPrompt = this.buildPrompt(cv, jobOffer);

    // Add user message to history
    history.push({
      role: 'user',
      content: userPrompt,
    });

    // Call OpenAI API with structured output
    const completion = await this.openai.chat.completions.create({
      model: 'gpt-4o-mini', // Using gpt-4o-mini as equivalent to gpt-4.1-nano
      messages: history,
      response_format: {
        type: 'json_schema',
        json_schema: {
          name: 'adapted_cv',
          strict: true,
          schema: this.getCVJsonSchema(),
        },
      },
      temperature: 0.7,
    });

    const assistantMessage = completion.choices[0].message;

    // Add assistant response to history
    history.push(assistantMessage);

    // Keep only last 10 messages to avoid context length issues
    if (history.length > 10) {
      this.conversationHistory.set(sessionId, history.slice(-10));
    }

    // Parse and return the adapted CV
    const adaptedCV = JSON.parse(assistantMessage.content || '{}');
    return adaptedCV.cv;
  }

  /**
   * Build the prompt for CV adaptation
   */
  private buildPrompt(cv: CVData, jobOffer: JobOfferData): string {
    return `Les données de mon CV : ${JSON.stringify(cv, null, 2)}

Les données d'un poste sur lequel je veux postuler :
${jobOffer.job_offer_raw}

Le nom du poste : ${jobOffer.job_offer_name}

L'entreprise qui recrute : ${jobOffer.job_offer_company}

Origine de l'offre : ${jobOffer.job_offer_origin}

Analyse cette offre d'emploi et adapte mon CV pour mettre en avant les compétences et expériences les plus pertinentes pour ce poste. Conserve toutes les informations du CV original, mais réorganise et reformule les sections pour maximiser la pertinence par rapport à l'offre. Retourne le CV adapté au format JSON avec la même structure que l'original.`;
  }

  /**
   * Get the JSON schema for structured output
   */
  private getCVJsonSchema(): any {
    return {
      type: 'object',
      properties: {
        cv: {
          type: 'object',
          properties: {
            name: { type: 'string' },
            title: { type: 'string' },
            location: { type: 'string' },
            phone: { type: 'string' },
            email: { type: 'string' },
            website: { type: 'string' },
            linkedin: { type: 'string' },
            summary: { type: 'string' },
            skills: {
              type: 'array',
              items: {
                type: 'object',
                properties: {
                  category: { type: 'string' },
                  items: {
                    type: 'array',
                    items: { type: 'string' },
                  },
                },
                required: ['category', 'items'],
                additionalProperties: false,
              },
            },
            experience: {
              type: 'array',
              items: {
                type: 'object',
                properties: {
                  title: { type: 'string' },
                  company: { type: 'string' },
                  location: { type: 'string' },
                  dates: { type: 'string' },
                  bullets: {
                    type: 'array',
                    items: { type: 'string' },
                  },
                },
                required: ['title', 'company', 'location', 'dates', 'bullets'],
                additionalProperties: false,
              },
            },
            projects: {
              type: 'array',
              items: {
                type: 'object',
                properties: {
                  title: { type: 'string' },
                  role: { type: 'string' },
                  bullets: {
                    type: 'array',
                    items: { type: 'string' },
                  },
                },
                required: ['title', 'role', 'bullets'],
                additionalProperties: false,
              },
            },
            education: {
              type: 'array',
              items: {
                type: 'object',
                properties: {
                  degree: { type: 'string' },
                  school: { type: 'string' },
                  location: { type: 'string' },
                  dates: { type: 'string' },
                  bullets: {
                    type: 'array',
                    items: { type: 'string' },
                  },
                },
                required: ['degree', 'school', 'location', 'dates', 'bullets'],
                additionalProperties: false,
              },
            },
          },
          required: [
            'name',
            'title',
            'location',
            'phone',
            'email',
            'website',
            'linkedin',
            'summary',
            'skills',
            'experience',
            'projects',
            'education',
          ],
          additionalProperties: false,
        },
      },
      required: ['cv'],
      additionalProperties: false,
    };
  }

  /**
   * Clear conversation history for a session
   */
  clearSession(sessionId: string): void {
    this.conversationHistory.delete(sessionId);
  }
}
