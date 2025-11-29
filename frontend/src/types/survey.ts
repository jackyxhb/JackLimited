export interface SurveyRequest {
  likelihoodToRecommend: number;
  comments?: string;
  email?: string;
}

export interface NpsResponse {
  nps: number;
}
