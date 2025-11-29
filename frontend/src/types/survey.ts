export interface SurveyRequest {
  likelihoodToRecommend: number;
  comments?: string;
  email?: string;
}

export interface NpsResponse {
  nps: number;
}

export interface AverageResponse {
  average: number;
}

export interface DistributionResponse {
  [key: number]: number;
}
