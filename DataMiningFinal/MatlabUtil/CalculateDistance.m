function S = CalculateDistance(p, method)
    S = pdist2(p, p, method);
    S(isnan(S)) = 0;
end